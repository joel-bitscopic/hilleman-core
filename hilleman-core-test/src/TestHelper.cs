using System;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain.pooling.connection.vista;
using com.bitscopic.hilleman.core.domain;
using System.Collections.Generic;
using System.Configuration;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core
{
    public static class TestHelper
    {
        public static IVistaConnection getConnectionFromConnectionPool(String sourceSystemId)
        {
            VistaRpcConnectionPoolsSource poolsSource = new VistaRpcConnectionPoolsSource();
            poolsSource.CxnSources = new Dictionary<string, VistaRpcConnectionPoolSource>();
            SourceSystemTable srcTable = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));

            ConnectionManager.getInstance();
            /*
            foreach (SourceSystem ss in srcTable.sources)
            {
                VistaRpcConnectionPoolSource src = new VistaRpcConnectionPoolSource()
                {
                    Timeout = new TimeSpan(0, 3, 0), // this should ALWAYS be larger than the connection READ_TIMEOUT!!
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
                poolsSource.CxnSources.Add(ss.id, src);
            }

            // starts the main pool process
            VistaRpcConnectionPools pools = (VistaRpcConnectionPools)new VistaRpcConnectionPoolFactory().getResourcePool(poolsSource);
            */

            return VistaRpcConnectionPools.getInstance().checkOutAlive(sourceSystemId) as IVistaConnection;
            //return pools.checkOutAlive(sourceSystemId) as IVistaConnection;
        }

        public static void returnConnection(IVistaConnection cxn)
        {
            VistaRpcConnectionPools.getInstance().checkIn((com.bitscopic.hilleman.core.domain.pooling.AbstractResource)cxn);
        }

        public static void cleanupAfterAllTests()
        {
            try
            {
                VistaRpcConnectionPools.getInstance().shutdown();
            }
            catch (Exception) { /* directly accessing VistaRpcConnectionPools.getInstance may throw an exception if the pool of pools was never instantiated - swallow here so our callers can remain ignorant */ }
        }
    }
}
