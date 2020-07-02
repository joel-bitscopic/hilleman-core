using System;
using System.Collections.Generic;
using System.Threading;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPools : AbstractResourcePool
    {
        static bool _isRunning = false;
        static byte SHUTDOWN_FLAG = 0;
        bool _starting = false;
        static readonly object _locker = new object();
        static readonly object _instantiationLocker = new object();

        internal Dictionary<string, VistaRpcConnectionPool> _pools;
        internal Dictionary<string, Thread> _poolThreads;

        #region Singleton
        public static VistaRpcConnectionPools getInstance()
        {
            if (_thePool == null)
            {
                throw new ArgumentException("Must call getInstance(AbstractPoolSource) before invoking the parameterless accessor");
            }
            return _thePool;
        }

        public static VistaRpcConnectionPools getInstance(AbstractPoolSource source)
        {
            if (_thePool == null)
            {
                lock (_instantiationLocker)
                {
                    if (_thePool == null)
                    {
                        _thePool = new VistaRpcConnectionPools(source);
                    }
                }
            }
            return _thePool;
        }
        static VistaRpcConnectionPools _thePool;
        private VistaRpcConnectionPools(AbstractPoolSource source) 
        {
            this.PoolSource = source;
            Thread poolThread = new Thread(new ThreadStart(this.run));
            poolThread.Name = "PoolOfPools";
            poolThread.IsBackground = true; // this allows the main process to terminate without the connection pool being forced to clean up - ok?
            _starting = true; // set here to avoid race conditions in unit tests
            poolThread.Start();
        }
        #endregion


        internal void run()
        {
            if (_isRunning)
            {
                return;
            }

            // never let two processes call run!!!
            lock (_locker)
            {
                _isRunning = true;
                try
                {
                    VistaRpcConnectionPoolsSource poolSource = (VistaRpcConnectionPoolsSource)this.PoolSource; // just cast this once
                    // startup
                    _starting = true;
                    startUp(poolSource);
                    _starting = false;

                    while (!Convert.ToBoolean(SHUTDOWN_FLAG))
                    {
                        // babysit pools
                        foreach (string siteId in poolSource.CxnSources.Keys)
                        {
                            if (_pools[siteId] == null) // if pool hasn't been instantiated
                            {
                                if (this.PoolSource.LoadStrategy == LoadingStrategy.Lazy) // and loading lazily then just continue
                                {
                                    continue;
                                }
                                else if (this.PoolSource.LoadStrategy == LoadingStrategy.Eager) // and loading eagerly then instantiate pool
                                {
                                    startPool(siteId, poolSource.CxnSources[siteId]);
                                }
                            }
                        }
                        System.Threading.Thread.Sleep(10000);
                    }
                }
                catch (Exception) 
                {
                    throw;
                }
                finally
                {
                    _isRunning = false;
                }
            }
        }

        void startUp(VistaRpcConnectionPoolsSource source)
        {
            // first things first - initialize the pool collection based off the connection sources!
            _pools = new Dictionary<string, VistaRpcConnectionPool>();
            _poolThreads = new Dictionary<string, Thread>();
            foreach (string siteId in source.CxnSources.Keys)
            {
                _pools.Add(siteId, null);
                _poolThreads.Add(siteId, null);
            }

            if (source.LoadStrategy == LoadingStrategy.Lazy)
            {
                return; // lazy loading? we're all done with startup!
            }

            // the rest of this code starts the pool for each of the connection sources
            string[] allKeys = new string[source.CxnSources.Count];
            source.CxnSources.Keys.CopyTo(allKeys, 0);
            IList<VistaRpcConnectionPool> allPools = new List<VistaRpcConnectionPool>(allKeys.Length);

            for (int i = 0; i < allKeys.Length; i++)
            {
                DateTime lastPoolStart = DateTime.Now;

                // starting 130+ connection pool threads takes a lot of system resources - we should try and let the 
                // previous pool come up or at least give it a reasonable time to start before moving to the next connection pool
                if (i > 0)
                {
                    while (lastPoolStart.Subtract(DateTime.Now).TotalSeconds < 60 &&
                        allPools[i - 1].TotalResources < allPools[i - 1].PoolSource.MinPoolSize)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }

                // go ahead and start the pool now
                startPool(allKeys[i], source.CxnSources[allKeys[i]]);
                allPools.Add(_pools[allKeys[i]]);
            }
        }

        /// <summary>
        /// Check a connection in to the pool
        /// </summary>
        /// <param name="objToReturn">The AbstractConnection to check in</param>
        /// <returns>null</returns>
        public override object checkIn(AbstractResource objToReturn)
        {
            if (objToReturn == null || (!(objToReturn is VistaRpcConnection) && !(objToReturn is VistaStatelessRpcConnection)))
            {
                throw new ArgumentException("Invalid object for checkin");
            }
            VistaRpcConnection theCxn = (VistaRpcConnection)objToReturn;
            if (theCxn.SourceSystem == null || String.IsNullOrEmpty(theCxn.SourceSystem.id))
            {
                throw new ArgumentException("The connection source is incomplete");
            }
            if (!_pools.ContainsKey(theCxn.SourceSystem.id))
            {
                throw new ArgumentException("No pool found for that connection");
            }
            _pools[theCxn.SourceSystem.id].checkIn(theCxn);
            return null;
        }

        /// <summary>
        /// Checkout a connection
        /// </summary>
        /// <param name="obj">The connection identifier (usually the site ID)</param>
        /// <returns>AbstractConnection</returns>
        public override AbstractResource checkOut(object obj)
        {
            while (_starting) // block while startup is occurring
            {
                System.Threading.Thread.Sleep(10);
            }
            if (!(obj is String))
            {
                throw new ArgumentException("Must supply the ID of the connection pool to check out a connection");
            }
            string site = (String)obj;
            // first make sure we have a dictionary key/queue for this site - if lazy loading then create a new pool for site - else exception
            VistaRpcConnectionPoolsSource source = (VistaRpcConnectionPoolsSource)this.PoolSource;
            if (!_pools.ContainsKey(site))
            {
                if (!source.CxnSources.ContainsKey(site))
                {
                    throw new ArgumentException("No configuration information available for that connection pool ID ({0}) - unable to start", site);
                }
                _pools.Add(site, null);
            }
            if (_pools[site] == null)
            {
                if (this.PoolSource.LoadStrategy == LoadingStrategy.Lazy)
                {
                    startPool(site, source.CxnSources[site]);
                    //_pools[site] = (ConnectionPool)ConnectionPoolFactory.getResourcePool(source.CxnSources[site]);
                }
                else // treating this as an error case - if we're not lazy loading and the pool hasn't already been initialized then assume pool is being used incorrectly
                {
                    throw new System.Configuration.ConfigurationErrorsException("The pools have not been initialized properly to service your request");
                }
            }
            // try and check out a connection from the pool
            return _pools[site].checkOut(null);
        }

        void startPool(String site, AbstractPoolSource source)
        {
            if (_pools[site] == null)
            {
                lock (_instantiationLocker) // need to lock here so multiple threads don't try creating the same pool
                {
                    if (_pools[site] == null) // and then check again after lock is entered
                    {
                        VistaRpcConnectionPool newPool = new VistaRpcConnectionPool();
                        newPool.PoolSource = source; 
                        Thread poolThread = new Thread(new ThreadStart(newPool.run));
                        poolThread.Name = "HillemanVistaRpcConnectionPool" + site;
                        poolThread.IsBackground = true;
                        poolThread.Start();
                        _pools[site] = newPool;
                        _poolThreads[site] = poolThread;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the shutdown signal to each pool. Marks this pool as shutting down also
        /// </summary>
        public override void  shutdown()
        {
            if (SHUTDOWN_FLAG == 1)
            {
                return;
            }
            SHUTDOWN_FLAG = 1;
            string[] allKeys = new string[_pools.Keys.Count];
            _pools.Keys.CopyTo(allKeys, 0);
            IList<Thread> shutdownThreads = new List<Thread>();
            for (int i = 0; i < _pools.Count; i++)
            {
                if (_pools[allKeys[i]] != null)
                {
                    Thread shutdownThread = new Thread(new ThreadStart(_pools[allKeys[i]].shutdown));
                    shutdownThreads.Add(shutdownThread);
                    shutdownThread.Start();
                }
            }
            foreach (Thread t in shutdownThreads)
            {
                try
                {
                    t.Join(60000); // give each shutdown thread a minute to complete
                }
                catch (Exception) { }
            }
        }

        public override AbstractResource checkOutAlive(object obj)
        {
            AbstractResource resource = checkOut(obj);
            while (!resource.isAlive())
            {
                _pools[(String)obj].decrementResourceCount(); // we decrement resource count for this pool here because checkOut doesn't do it
                resource = checkOut(obj);
            }
            return resource;
        }
    }
}
