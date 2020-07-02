using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPool : AbstractResourcePool
    {
        protected byte SHUTDOWN_FLAG = 0;
        private readonly object _locker = new object(); // don't make static because there may be many pools

        IList<VistaRpcConnectionThread> _startedCxns = new List<VistaRpcConnectionThread>();
        IList<VistaRpcConnectionThread> _cxnsToRemove = new List<VistaRpcConnectionThread>();

        internal BlockingCollection<AbstractResource> _pooledCxns = new BlockingCollection<AbstractResource>();
        DateTime _startupTimestamp;

        DateTime _lastSuccessfulCxn = DateTime.Now;
        Int32 _consecutiveCxnErrorCount = 0;

        IList<Task> _cleanupTasks = new List<Task>();

        /// <summary>
        /// This method removes an object from the pool
        /// </summary>
        /// <param name="obj">Not currently used</param>
        /// <returns>AbstractConnection</returns>
        public override AbstractResource checkOut(object obj)
        {
            AbstractResource cxn = null;
            if (!_pooledCxns.TryTake(out cxn, this.PoolSource.WaitTime))
            {
               // System.Console.WriteLine("Unable to remove connection from pool - were " + _pooledCxns.Count + " cxns available - total resource count: " + this.TotalResources);
                throw new TimeoutException("No connection could be obtained in the configured allotment");
            }
            return cxn;
        }

        /// <summary>
        /// Return an AbstractConnection to the pool
        /// </summary>
        /// <param name="cxn">The connection to return to the pool</param>
        /// <returns></returns>
        public override object checkIn(AbstractResource cxn)
        {
            VistaRpcConnection theCxn = (VistaRpcConnection)cxn; // first get a new reference
            if (!theCxn.IsConnected || !theCxn.isAlive()) // don't add disconnected connections to the pool
            {
                this.decrementResourceCount();
                return null;
            }
            theCxn.LastUsed = DateTime.Now;

            _pooledCxns.Add(theCxn);
            return null;
        }

        /// <summary>
        /// The job of the run function is simply to make sure we have connections available in the pool. If the # of 
        /// connections falls below the threshold, the pool expands (up to the limit). The loop also tries to clean
        /// up any connections that may have failed
        /// </summary>
        internal void run()
        {
            lock (_locker)
            {
                _startupTimestamp = DateTime.Now;

                while (!Convert.ToBoolean(SHUTDOWN_FLAG))
                {
                    System.Threading.Thread.Sleep(500); // this small sleep time prevents the thread from consuming 100% of CPU

                    // this first IF statement checks to see if more connections need to be added to the pool
                    if (_pooledCxns.Count < this.PoolSource.MinPoolSize && _startedCxns.Count == 0) // only grow if we haven't started any connections
                    {
                        if (this.TotalResources < this.PoolSource.MaxPoolSize)
                        {
                            growPool();
                        }
                    }

                    // the second IF checks if this pool has started any connections - most of the time this should be false so we check it before the getEnumerator call
                    if (_startedCxns.Count > 0)
                    {
                        IEnumerator<VistaRpcConnectionThread> enumerator = _startedCxns.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            VistaRpcConnectionThread current = enumerator.Current;
                            Thread t = current.Thread;
                            VistaRpcConnection currentCxn = (VistaRpcConnection)current.Connection;
                            if (!(t.IsAlive) && currentCxn.isAlive()) // check if started connection is ready for our pool
                            {
                                try { t.Join(0); } catch (Exception) { } 
                                //Console.WriteLine("Found successfully started connection");
                                this.incrementResourceCount();
                                _pooledCxns.Add(currentCxn);
                                _cxnsToRemove.Add(current);
                            }
                            else if (!(t.IsAlive) && !currentCxn.isAlive()) // check if started connection thread has completed but for any reason disconnected
                            {
                                try { t.Join(0); } catch (Exception) { }
                                _cxnsToRemove.Add(current);
                            }
                            else if (DateTime.Now.Subtract(current.Timestamp).TotalSeconds > this.PoolSource.WaitTime.TotalSeconds) // lastly check for long running connection attempts
                            {
                                _cxnsToRemove.Add(current);

                                try
                                {                                    
                                    // create async task to wait for the thread to stop and disconnect the cxn just to be sure things are cleaned up
                                    Task disconnectTask = new System.Threading.Tasks.Task(() => 
                                    { 
                                        t.Join(this.PoolSource.WaitTime); // should be enough - waiting for double pool wait time in total
                                        disconnect(current.Connection);
                                    });
                                    _cleanupTasks.Add(disconnectTask);
                                    disconnectTask.Start();
                                }
                                catch (AggregateException) { } 
                                catch (Exception) { /* swallow */ }
                            }
                            
                        }
                    }

                    // per previous IF - can't modify collection while enumerating so the removal of failed connections is a separate step
                    if (_cxnsToRemove.Count > 0)
                    {
                        foreach (VistaRpcConnectionThread t in _cxnsToRemove)
                        {
                            _startedCxns.Remove(t);
                        }
                        _cxnsToRemove = new List<VistaRpcConnectionThread>();
                    }

                    try { checkCleanupTasks(); } catch (Exception) { }
                }
            }
        }

        void checkCleanupTasks()
        {
            bool allCleanupTasksNull = true;
            String poolSiteId = ((VistaRpcConnectionPoolSource)this.PoolSource).CxnSource.id;
            for (int i = 0; i < _cleanupTasks.Count; i++)
            {
                if (_cleanupTasks[i] == null)
                {
                    continue;
                }
                if (_cleanupTasks[i].IsFaulted)
                {
                    _cleanupTasks[i] = null;
                }
                else if (_cleanupTasks[i].IsCompleted)
                {
                    _cleanupTasks[i] = null;
                }
                else // if task not
                {
                    allCleanupTasksNull = false;
                }
            }
            // if all cleanup tasks have been addressed then create a new list!
            if (allCleanupTasksNull && _cleanupTasks.Count > 0)
            {
                _cleanupTasks = new List<Task>();
            }
        }

        void growPool()
        {
            if (_consecutiveCxnErrorCount > this.PoolSource.MaxConsecutiveErrors) // we want to recognize when a site might be down, network issues, etc. - wait to retry if we have many connection failures without success
            {
                if (DateTime.Now.Subtract(_lastSuccessfulCxn).CompareTo(this.PoolSource.WaitOnMaxConsecutiveErrors) < 0)
                {
                    return; // don't start any new cxns if we've seen a lot of errors and haven't waited at least 5 mins (or configurable timespan)
                }
                else // waited configured time, reset error vars so growPool will try creating cxns
                {
                    _consecutiveCxnErrorCount = 0;
                    _lastSuccessfulCxn = DateTime.Now;
                }
            }
            if ((_cleanupTasks.Count + this.TotalResources) >= this.PoolSource.MaxPoolSize)
            {
                return; // too many cleanup tasks scheduled! don't grow the pool until the cxns we've attempted to start have been addressed
            }
            // i think there may be a possible race condition here where a cxn may be disconnected during the size
            if (this.PoolSource.PoolExpansionSize <= 0)
            {
                this.PoolSource.PoolExpansionSize = 1;
            }
            if (this.PoolSource.MaxPoolSize <= 0)
            {
                this.PoolSource.MaxPoolSize = 1;
            }
            int growSize = this.PoolSource.PoolExpansionSize;
            if ((this.TotalResources + growSize) > this.PoolSource.MaxPoolSize) // if the growth would expand the pool above the max pool size, only grow by the amount allowed
            {
                growSize = this.PoolSource.MaxPoolSize - this.TotalResources;
            }
            for (int i = 0; i < growSize; i++)
            {
                VistaRpcConnectionThread a = new VistaRpcConnectionThread();
                _startedCxns.Add(a);
                a.Connection = VistaPooledRpcConnectionFactory.getInstance().getConnection(this.PoolSource); // new VistaStatelessRpcConnection(((VistaRpcConnectionPoolSource)this.PoolSource).CxnSource);
                a.Source = this.PoolSource;
                // from testing against local network vista w/out change to config ((VistaRpcConnectionPoolSource)a.Source).CxnSource.connectionString = "192.168.0.115:9430";
                Thread t = new Thread(new ParameterizedThreadStart(connect));
                a.Thread = t;
                t.Start(a);
            }
        }

        void connect(object obj)
        {
            VistaRpcConnectionThread cxnThread = (VistaRpcConnectionThread)obj;
            try
            {
                cxnThread.Connection.connect();
                VistaRpcConnectionPoolSource rpcPoolSrc = (VistaRpcConnectionPoolSource)cxnThread.Source;
                //rpcPoolSrc.Credentials
                if (rpcPoolSrc.Credentials != null) // should be an authenticated connection!
                {
                    if (rpcPoolSrc.Credentials is VistaRpcLoginCredentials)
                    {
                        new VistaRpcCrrudDao(cxnThread.Connection).login((VistaRpcLoginCredentials)rpcPoolSrc.Credentials, rpcPoolSrc.BrokerContext);
                    }
                    else if (rpcPoolSrc.Credentials is VistaRpcVisitorCredentials)
                    {
                        VistaRpcVisitorCredentials visitorCreds = (VistaRpcVisitorCredentials)rpcPoolSrc.Credentials;
                        new VistaRpcCrrudDao(cxnThread.Connection).visit(visitorCreds.visitor, visitorCreds, rpcPoolSrc.BrokerContext);
                    }
                    else
                    {
                        throw new NotImplementedException("That credential type is not yet supported");
                    }
                }
                _lastSuccessfulCxn = DateTime.Now;
                _consecutiveCxnErrorCount = 0;
                ((VistaRpcConnection)cxnThread.Connection).setTimeout(this.PoolSource.Timeout); // now gracefully timing out connections!
            }
            catch (Exception exc)
            {
                utils.LogUtils.LOG(
                    String.Format("Problem connecting in pool {0}: {1}", 
                    (((VistaRpcConnectionPoolSource)cxnThread.Source).CxnSource.id), 
                    exc.ToString()));
                _consecutiveCxnErrorCount++;
                disconnect(cxnThread.Connection);
            }
        }

        void disconnect(object vistaRpcConnection)
        {
            try
            {
                ((VistaRpcConnection)vistaRpcConnection).disconnect();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Signal the pool to shutdown. An attempt will be made to wait for as many connections as possible for return to the pool 
        /// before disconnecting each of the connections. Sets SHUTDOWN_FLAG so pool no longer tries to continue to run
        /// </summary>
        public override void shutdown()
        {
            if (SHUTDOWN_FLAG == 1)
            {
                return;
            }
            SHUTDOWN_FLAG = 1;

            AbstractResource current = null;
            while (_pooledCxns.TryTake(out current, 1000))
            {
                ((VistaRpcConnection)current).disconnect();
            }
        }

        /// <summary>
        /// Check the status of the pool. If the pool has been signalled to shutdown, this should return false. If the pool
        /// has no available resources and has been running for more than one minute, assume something went wrong and return false.
        /// Otherwise, this should return true
        /// </summary>
        public bool IsAlive 
        {
            get
            {
                if (Convert.ToBoolean(SHUTDOWN_FLAG))
                {
                    return false;
                }
                if ((TotalResources == 0 && (_startedCxns == null || _startedCxns.Count == 0)) && // if no total resources AND no started connections
                    DateTime.Now.Subtract(_startupTimestamp).CompareTo(this.PoolSource.WaitTime) > 0) // if we have no resources, no started resources AND pool started more than 60 seconds ago
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Check out an object from the pool and call isAlive to guarantee state. Decrement resource count
        /// on pool if object is not alive and discard it by removing references
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>AbstractResource</returns>
        public override AbstractResource checkOutAlive(object obj)
        {
            AbstractResource resource = checkOut(obj);
            while (!resource.isAlive())
            {
                this.decrementResourceCount();
                resource = checkOut(obj);
            }
            return resource;
        }
    }
}
