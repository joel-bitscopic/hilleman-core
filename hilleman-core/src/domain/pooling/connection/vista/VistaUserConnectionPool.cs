using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaUserConnectionPool : AbstractResourcePool
    {
        const Int16 MAX_CXNS_PER_USER_PER_SITE = 2;
        SourceSystemTable _sources = null;
        VistaRpcConnectionPoolSource _defaultConfigSource = null;
        ConcurrentDictionary<String, ConcurrentDictionary<String, VistaRpcConnectionPool>> _connectionPoolsByUserAndSite = null;
        private static readonly object _locker = new object();
        private static ConcurrentDictionary<String, object> _userCxnLocker = null; // don't want to make connection creation serial for all users so organize this by user so only lock per user

        #region Singleton
        public static VistaUserConnectionPool getInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new VistaUserConnectionPool();
                    }
                }
            }

            return _instance;
        }

        private static VistaUserConnectionPool _instance = null;

        private VistaUserConnectionPool()
        {
            _userCxnLocker = new ConcurrentDictionary<string, object>();
            _sources = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
            _connectionPoolsByUserAndSite = new ConcurrentDictionary<string, ConcurrentDictionary<string, VistaRpcConnectionPool>>();
            if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("PG_VistaUserCxnPoolConfigSource")))
            {
                _defaultConfigSource = SerializerUtils.deserialize<VistaRpcConnectionPoolSource>(MyConfigurationManager.getValue("PG_VistaUserCxnPoolConfigSource"));
            }
            else
            {
                _defaultConfigSource = new VistaUserRpcConnectionPoolSource()
                                    {
                                        Timeout = new TimeSpan(0, 4, 55),
                                        WaitTime = new TimeSpan(0, 0, 15),
                                        MaxPoolSize = MAX_CXNS_PER_USER_PER_SITE,
                                        MinPoolSize = 1,
                                        PoolExpansionSize = 1
                                    };
            }
        }

        #endregion

        public override object checkIn(AbstractResource objToReturn)
        {
            if (!(objToReturn is VistaUserRpcConnection))
            {
                throw new ArgumentException("Must return a VistA user RPC connection to this pool");
            }

            VistaUserRpcConnection cxn = (VistaUserRpcConnection)objToReturn;
            String cxnSiteId = cxn.getSource().id;

            if (cxn.user == null || String.IsNullOrEmpty(cxn.user.id)) // does the connection have a valid user?
            {
                cxn.disconnect();
                throw new ArgumentException("That connection is missing the required user info! The connection has been terminated");
            }

            if (!_connectionPoolsByUserAndSite.ContainsKey(cxn.user.id)) // has this user been set up already?
            {
                cxn.disconnect();
                throw new ArgumentException("That connection user was not found! Unable to return the connection. The connection has been terminated");
            }

            if (_connectionPoolsByUserAndSite[cxn.user.id] == null || !_connectionPoolsByUserAndSite[cxn.user.id].ContainsKey(cxnSiteId)) // does this user have connections?
            {
                cxn.disconnect();
                throw new ArgumentException("That user doesn't have any connections. You must check in connections for the correct user. The connection has been terminated");
            }

            // OK!! set cxn to 'available' to this connection can be checked out again
            cxn.LastUsed = DateTime.Now;
            cxn.isAvailable = true;

            _connectionPoolsByUserAndSite[cxn.user.id][cxnSiteId].checkIn(cxn);

            return "OK";
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override AbstractResource checkOut(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch a pooled connection by user. must specify user.id and user.sourceSystemId (for site to which a connection should be made available)
        /// where the user.id is a guaranteed unique identifier (e.g. email address, GUID, etc). 
        /// </summary>
        /// <param name="obj">Must pass: full VistA name, SSN, home site ID/DUZ combo and user's phone</param>
        /// <returns></returns>
        public override AbstractResource checkOutAlive(object obj)
        {
            if (!(obj is User))
            {
                throw new ArgumentException("Must supply user for this resouce pool");
            }

            User user = (User)obj;

            if (!_connectionPoolsByUserAndSite.ContainsKey(user.id) || !_connectionPoolsByUserAndSite[user.id].ContainsKey(user.sourceSystemId)) // has user's ID been added to pool?
            {
                initilizeUsersConnectionPoolForSite(user);
            }

            return _connectionPoolsByUserAndSite[user.id][user.sourceSystemId].checkOutAlive(obj);
        }

        internal void initilizeUsersConnectionPoolForSite(User user)
        {
            if (!_connectionPoolsByUserAndSite.ContainsKey(user.id))
            {
                _userCxnLocker.TryAdd(user.id, new object());
                _connectionPoolsByUserAndSite.TryAdd(user.id, new ConcurrentDictionary<string, VistaRpcConnectionPool>());
            }

            lock (_userCxnLocker[user.id])
            {
                if (!_connectionPoolsByUserAndSite[user.id].ContainsKey(user.sourceSystemId))
                {
                    VistaRpcConnectionPool pool = new VistaRpcConnectionPool();

                    SourceSystem usersSite = _sources.getSourceSystem(user.idSet.getByName("DUZ").sourceSystemId);
                    SourceSystem visitSite = _sources.getSourceSystem(user.sourceSystemId);

                    User visitor = new User();
                    visitor.idSet = new IdentifierSet() { ids = new List<Identifier>() };
                    visitor.idSet.add(user.idSet.getByName("SSN").id, "FEDID");
                    visitor.idSet.add(user.idSet.getByName("DUZ").id, "PROVIDER");
                    visitor.phones = user.phones;
                    if (user.phones == null || user.phones.Count == 0)
                    {
                        //visitor.phones = new Dictionary<string, string>();
                        visitor.phones.Add("office", "no phone");
                    }

                    VistaRpcVisitorCredentials creds = new VistaRpcVisitorCredentials();
                    creds.username = user.nameString;
                    creds.provider = usersSite;
                    creds.visitor = visitor;

                    VistaUserRpcConnectionPoolSource src = new VistaUserRpcConnectionPoolSource()
                    {
                        Timeout = _defaultConfigSource.Timeout, // new TimeSpan(0, 4, 55),
                        WaitTime = _defaultConfigSource.WaitTime, // new TimeSpan(0, 0, 15),
                        MaxPoolSize = _defaultConfigSource.MaxPoolSize, // MAX_CXNS_PER_USER_PER_SITE,
                        MinPoolSize = _defaultConfigSource.MinPoolSize, // 1,
                        PoolExpansionSize = _defaultConfigSource.PoolExpansionSize, // 1,
                        CxnSource = visitSite,
                        Credentials = creds,
                        EndUser = user
                    };

                    pool.PoolSource = src;
                    Task.Run(() => pool.run());

                    DateTime startWait = DateTime.Now;
                    while (!pool.IsAlive)
                    {
                        System.Threading.Thread.Sleep(100);
                        if (DateTime.Now.Subtract(startWait).TotalSeconds > 15)
                        {
                            throw new VistaRpcConnectionException("Unable to start a user connection pool for site " + user.sourceSystemId);
                        }
                    }
                    _connectionPoolsByUserAndSite[user.id].TryAdd(user.sourceSystemId, pool);
                }
            }
        }

        public override void shutdown()
        {
            foreach (String userId in _connectionPoolsByUserAndSite.Keys)
            {
                foreach (var pool in _connectionPoolsByUserAndSite[userId].Values)
                {
                    if (pool != null && pool.IsAlive)
                    {
                        try
                        {
                            new Action(() => pool.shutdown()).Invoke();
                        }
                        catch (AggregateException ae) { var trash = ae.GetBaseException(); /* swallow */ }
                        catch (Exception) { /* swallow */ }
                    }
                }
            }

            _instance = VistaUserConnectionPool.getInstance(); // reset pool!
        }
    }
}