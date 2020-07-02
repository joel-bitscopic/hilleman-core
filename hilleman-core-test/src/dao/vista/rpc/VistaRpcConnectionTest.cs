using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    [TestFixture]
    public class VistaRpcConnectionTest
    {
        [Test]
        public void testConnect()
        {
            VistaRpcConnection cxn = new VistaRpcConnection(
                new domain.SourceSystem()
                {
                    connectionString = "127.0.0.1:9200",
                    id = "901",
                    name = "CPM",
                    type = domain.SourceSystemType.VISTA_RPC_BROKER
                });
            cxn.connect();
            cxn.disconnect();
        }

        [Test]
        //[ExpectedException(typeof(ArgumentException), ExpectedMessage = "Invalid source connection string")]
        public void testConnectInvalidConnectionString()
        {
            VistaRpcConnection cxn = new VistaRpcConnection(
                new domain.SourceSystem()
                {
                    connectionString = "127.0.0.1", // note no port #
                    id = "901",
                    name = "CPM",
                    type = domain.SourceSystemType.VISTA_RPC_BROKER
                });
            cxn.connect();
            cxn.disconnect();
        }

        [Test]
        //[ExpectedException(typeof(VistaRpcConnectionException), ExpectedMessage = "There doesn't appear to be a VistA listener at 127.0.0.1:1234")]
        public void testConnectNonExistentConnectionString()
        {
            VistaRpcConnection cxn = new VistaRpcConnection(
                new domain.SourceSystem()
                {
                    connectionString = "127.0.0.1:1234", // made up port #
                    id = "901",
                    name = "CPM",
                    type = domain.SourceSystemType.VISTA_RPC_BROKER
                });
            cxn.connect();
            cxn.disconnect();
        }
    }
}
