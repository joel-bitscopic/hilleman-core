using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.dao.vista.http;
using com.bitscopic.hilleman.core.domain.pooling.connection.vista;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System.IO;
using com.bitscopic.hilleman.core.utils;
using System.Configuration;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.refactoring;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain.resource;

namespace com.bitscopic.hilleman.core.domain
{
    public class ConnectionManager
    {
        #region Singleton

        private static readonly object locker = new object();
        private static ConnectionManager _cxnMgr;

        public static ConnectionManager getInstance()
        {
            if (_cxnMgr == null)
            {
                lock (locker)
                {
                    if (_cxnMgr == null)
                    {
                        _cxnMgr = new ConnectionManager();
                    }
                }
            }
            return _cxnMgr;
        }

        // singleton - only constructed internally
        private ConnectionManager()
        {
            _sourceSystems = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
            startPools();
           // System.Threading.Thread.Sleep(500); // TODO - remove this!!! Just giving cxn pools a short amount of time to start 
        }
        #endregion

        void startPools()
        {
            VistaRpcConnectionPoolsSource poolsSource = new VistaRpcConnectionPoolsSource();
            poolsSource.CxnSources = new Dictionary<string, VistaRpcConnectionPoolSource>();
            SourceSystemTable srcTable = _sourceSystems;

            foreach (SourceSystem ss in srcTable.sources)
            {
                VistaRpcConnectionPoolSource src = new VistaRpcConnectionPoolSource()
                {
                    Timeout = new TimeSpan(0, 1, 0),
                    WaitTime = new TimeSpan(0, 1, 0),
                    MaxPoolSize = 4,
                    MinPoolSize = 1,
                    PoolExpansionSize = 1,
                    CxnSource = ss,
                    Credentials = new VistaRpcLoginCredentials()
                    {
                        username = ss.credentials.username,
                        password = ss.credentials.password
                    }
                };

                //<add key="FederatedVisitorPermission" value="DVBA CAPRI GUI"/>

                // if configured to enable connection pool via visit and no A/V codes, set up params for visitor account
                if (StringUtils.parseBool(MyConfigurationManager.getValue("FederatedVisitorEnabled")) && String.IsNullOrEmpty(src.Credentials.password))
                {
                    if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("FederatedVisitorPermission")))
                    {
                        src.BrokerContext = new VistaRpcConnectionBrokerContext("", MyConfigurationManager.getValue("FederatedVisitorPermission"));
                    }
                    src.Credentials = new VistaRpcVisitorCredentials()
                    {
                        username = MyConfigurationManager.getValue("FederatedVisitorUserName"),
                        provider = new SourceSystem() 
                        { 
                            id = MyConfigurationManager.getValue("FederatedVisitorProviderSiteId"),
                            name = MyConfigurationManager.getValue("FederatedVisitorProviderSiteName")
                        },
                        visitor = new User()
                        {
                            idSet = new IdentifierSet() 
                            { 
                                ids = new List<Identifier>() 
                                {
                                    new Identifier() { name = "FEDID", id = MyConfigurationManager.getValue("FederatedVisitorFedID") },
                                    new Identifier() { name = "PROVIDER", id = MyConfigurationManager.getValue("FederatedVisitorProviderSiteUserId") }
                                }
                            },
                            phones = new Dictionary<string,string>() { { "WORK", "FederatedVisitorProviderSitePhone" } }
                        }
                    };
                }
                //end visitor setup

                poolsSource.CxnSources.Add(ss.id, src);
            }

            // starts the main pool process
            VistaRpcConnectionPools pools = (VistaRpcConnectionPools)new VistaRpcConnectionPoolFactory().getResourcePool(poolsSource);
        }

        SourceSystemTable _sourceSystems;

        public IVistaConnection getVistaConnection(String sourceId)
        {
            SourceSystem src = _sourceSystems.getSourceSystem(sourceId);
            if (src.type == SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return new VistaHttpRestConnection(src);
            }
            else if (src.type == SourceSystemType.VISTA_RPC_BROKER)
            {
                return (VistaStatelessRpcConnection)VistaRpcConnectionPools.getInstance().checkOutAlive(sourceId);
            }
            else if (src.type == SourceSystemType.SQLITE_CACHE)
            {
                return new com.bitscopic.hilleman.core.dao.vista.sql.sqlite.VistaSqliteCacheConnection(src);
            }
            else
            {
                throw new NotImplementedException("That source ID does not appear to be valid");
            }
        }

        public IConnection getConnection(String sourceId)
        {
            SourceSystem src = _sourceSystems.getSourceSystem(sourceId);
            if (src.type == SourceSystemType.SQLITE)
            {
                return new com.bitscopic.hilleman.core.dao.sql.SqliteConnection(src);
            }
            else
            {
                throw new NotImplementedException("That source ID does not appear to be valid");
            }
        }

        /// <summary>
        /// Make a non-VistA query (e.g. fetch SourceSystemTable)
        /// </summary>
        /// <param name="functionToInvoke"></param>
        /// <param name="constructorArgs"></param>
        /// <param name="functionArgs"></param>
        /// <returns></returns>
        public Stream makeQuery(Delegate functionToInvoke, object[] functionArgs)
        {
            try
            {
                object result = functionToInvoke.DynamicInvoke(functionArgs);
                //object dao = constructorArgs == null ? Activator.CreateInstance(functionToInvoke.Target.GetType()) : Activator.CreateInstance(functionToInvoke.Target.GetType(), constructorArgs);
                //object result = functionToInvoke.Method.Invoke(dao, functionArgs);
                return SerializerUtils.serializeToStream(result);
            }
            catch (Exception exc)
            {
                return SerializerUtils.serializeToStream(new com.bitscopic.hilleman.core.domain.exception.ServiceResponseException() { innerException = exc });
            }
        }

        public T queryResource<T>(String token, String resourceID, Delegate functionToInvoke, object[] args)
        {
            HillemanRequest myRequest = new HillemanRequest()
            {
                requestName = functionToInvoke.Method.Name,
                requestTimestamp = DateTime.Now,
                args = args
            };
            HillemanSession mySession = null;
            IConnection cxn = null;

            try
            {
                if (String.IsNullOrEmpty(resourceID))
                {
                    throw new ArgumentNullException("You must supply a target site for your query");
                }

                Token myToken = TokenStoreFactory.getTokenStore().getToken(token);
                if (myToken == null)
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }
                mySession = (HillemanSession)myToken.state;

                if (!mySession.authorizedConnections.Contains(resourceID))
                {
                    throw new UnauthorizedAccessException("Your session has not been authorized to make calls to that source system");
                }

                cxn = this.getConnection(resourceID);

                prepareICxnForUse(cxn, mySession);

                object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), cxn); // create an instance of the refactoring API with HillemanSession from token
                //object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), mySession); // create an instance of the refactoring API with HillemanSession from token
                //((IRefactoringApi)dao).setTarget(cxn); // set the target connection for the IRefactoringApi implementation
                object result = functionToInvoke.Method.Invoke(dao, args); // finally call the method
                return (T)result;
            }
            catch (Exception exc)
            {
                if (exc.InnerException != null) // calling function via delegate usually results in TargetInvocationException and original exc gets hidden in inner
                {
                    throw exc.InnerException;
                }
                throw exc;
            }
            finally
            {
                myRequest.responseTimestamp = DateTime.Now; // always set the response - even if there was an exception (error may have come from target!)
                if (mySession != null)
                {
                    mySession.addRequest(myRequest); // add the request to the session
                  //  prepareCxnForReturn(cxn, mySession);
                  //  this.returnVistaConnection(cxn); // return the connection to the pool, if needed
                }
            }
        }

        public T makeQuery<T>(String token, String targetSiteId, Delegate functionToInvoke, object[] args)
        {
            HillemanRequest myRequest = new HillemanRequest()
            {
                requestName = functionToInvoke.Method.Name,
                requestTimestamp = DateTime.Now,
                args = args
            };
            HillemanSession mySession = null;
            IVistaConnection cxn = null;

            try
            {
                if (String.IsNullOrEmpty(targetSiteId))
                {
                    throw new ArgumentNullException("You must supply a target site for your query");
                }

                Token myToken = TokenStoreFactory.getTokenStore().getToken(token);
                if (myToken == null)
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }
                mySession = (HillemanSession)myToken.state;

                if (String.IsNullOrEmpty(targetSiteId))
                {
                    targetSiteId = mySession.getBaseConnection().getSource().id;
                }

                if (!mySession.authorizedConnections.Contains(targetSiteId))
                {
                    throw new UnauthorizedAccessException("Your session has not been authorized to make calls to that source system");
                }

                cxn = this.getVistaConnection(targetSiteId);

                prepareCxnForUse(cxn, mySession);

                object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), cxn); // create an instance of the refactoring API with HillemanSession from token
                //object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), mySession); // create an instance of the refactoring API with HillemanSession from token
                //((IRefactoringApi)dao).setTarget(cxn); // set the target connection for the IRefactoringApi implementation
                object result = functionToInvoke.Method.Invoke(dao, args); // finally call the method
                return (T)result;
            }
            catch (Exception exc)
            {
                if (exc.InnerException != null) // calling function via delegate usually results in TargetInvocationException and original exc gets hidden in inner
                {
                    throw exc.InnerException;
                }
                throw exc;
            }
            finally
            {
                myRequest.responseTimestamp = DateTime.Now; // always set the response - even if there was an exception (error may have come from target!)
                if (mySession != null)
                {
                    mySession.addRequest(myRequest); // add the request to the session
                    prepareCxnForReturn(cxn, mySession);
                    this.returnVistaConnection(cxn); // return the connection to the pool, if needed
                }
            }
        }

        /// <summary>
        /// Make a query to VistA (using connection pool if connection type is RPC Broker).
        /// The functionToInvoke delegate should be in a callable state.
        /// </summary>
        /// <param name="targetSiteId"></param>
        /// <param name="functionToInvoke">Should be a member of a IRefactoringApi implementation</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Stream makeQuery(String token, String targetSiteId, Delegate functionToInvoke, object[] args, bool throwOnError = false)
        {
            HillemanRequest myRequest = new HillemanRequest()
            {
                requestName = functionToInvoke.Method.Name,
                requestTimestamp = DateTime.Now,
                args = args
            };
            HillemanSession mySession = null;
            IVistaConnection cxn = null;

            try
            {
                if (String.IsNullOrEmpty(targetSiteId))
                {
                    throw new ArgumentNullException("You must supply a target site for your query");
                }

                Token myToken = TokenStoreFactory.getTokenStore().getToken(token);
                if (myToken == null)
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }
                mySession = (HillemanSession)myToken.state;

                if (String.IsNullOrEmpty(targetSiteId))
                {
                    targetSiteId = mySession.getBaseConnection().getSource().id;
                }

                if (!mySession.authorizedConnections.Contains(targetSiteId))
                {
                    throw new UnauthorizedAccessException("Your session has not been authorized to make calls to that source system");
                }

                if (mySession.getBaseConnection() is VistaStatefulRpcConnection)
                {
                    cxn = mySession.getBaseConnection();
                }
                else
                {
                    cxn = this.getVistaConnection(targetSiteId);
                }

                prepareCxnForUse(cxn, mySession);

                object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), cxn); // create an instance of the refactoring API with HillemanSession from token
                //object dao = Activator.CreateInstance(functionToInvoke.Target.GetType(), mySession); // create an instance of the refactoring API with HillemanSession from token
                //((IRefactoringApi)dao).setTarget(cxn); // set the target connection for the IRefactoringApi implementation
                object result = functionToInvoke.Method.Invoke(dao, args); // finally call the method
                return SerializerUtils.serializeToStream(result);
            }
            catch (Exception exc)
            {
                if (throwOnError)
                {
                    if (exc.InnerException != null) // calling function via delegate usually results in TargetInvocationException and original exc gets hidden in inner
                    {
                        throw exc.InnerException;
                    }
                    throw exc;
                }
                return SerializerUtils.serializeToStream(new com.bitscopic.hilleman.core.domain.exception.ServiceResponseException() { message = exc.Message, isException = true, innerException = exc });
            }
            finally
            {
                myRequest.responseTimestamp = DateTime.Now; // always set the response - even if there was an exception (error may have come from target!)
                if (mySession != null)
                {
                    mySession.addRequest(myRequest); // add the request to the session
                    prepareCxnForReturn(cxn, mySession);
                    if (cxn is VistaStatelessRpcConnection)// return the connection to the pool, if needed
                    {
                        this.returnVistaConnection(cxn); 
                    }
                }
            }
        }

        private void prepareCxnForReturn(IVistaConnection cxn, HillemanSession mySession)
        {
            //mySession.syncSymbolTable(cxn);
        }

        private void prepareCxnForUse(IVistaConnection cxn, HillemanSession mySession)
        {
            //mySession.setVistaSymbolTable(cxn);
        }

        private void prepareICxnForUse(IConnection cxn, HillemanSession mySession)
        {
            //mySession.setVistaSymbolTable(cxn);
        }

        private void returnVistaConnection(IVistaConnection cxn)
        {
            if (cxn != null && cxn.getSource() != null 
                && cxn.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                if (cxn is VistaStatelessRpcConnection)
                {
                    VistaRpcConnectionPools.getInstance().checkIn((VistaStatelessRpcConnection)cxn);
                }
                else
                {
                    throw new ArgumentException("Invalid connection for pool - don't return stateful connections to the pool!");
                }
            }
            // else do nothing!
        }


        /// <summary>
        /// Find the token and return the state object from it (usually HillemanSession)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        internal HillemanSession getSessionFromToken(string token)
        {
            Token myToken = TokenStoreFactory.getTokenStore().getToken(token);
            return (HillemanSession)myToken.state;
        }
    }
}