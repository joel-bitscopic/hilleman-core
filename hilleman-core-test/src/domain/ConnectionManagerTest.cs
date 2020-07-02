using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.utils;
using System.Configuration;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.refactoring;
using System.IO;

namespace com.bitscopic.hilleman.core.domain
{
    [TestFixture]
    public class ConnectionManagerTest
    {
        SourceSystemTable _sourceTable = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
        String _token = "";

        [OneTimeSetUp]
        public void tfsu()
        {
            String sst = FileIOUtils.readFile(@"C:\workspace\assembla\svn\hilleman\branches\joel\hilleman-cs-test\resources\testing\SymbolTable.dat").Trim();
            HillemanSession mySession = new HillemanSession()
            {
                endUser = new User() { id = "1", nameString = "USER,VISTA" },
                plaintextSerializedSymbolTable = sst,
                encryptedSerializedSymbolTable = CryptographyUtils.hmac256Hash(MyConfigurationManager.getValue("EncryptionKey"), sst)
            };
            mySession.addAuthorizedConnection(new VistaStatelessRpcConnection(_sourceTable.getSourceSystem("901")));
            Token userToken = TokenStoreFactory.getTokenStore().createNewToken(mySession);
            mySession.sessionToken = userToken.value;
            _token = userToken.value; // set so tests can pass this to connection manager
        }

        [OneTimeTearDown]
        public void tftd()
        {
        }

        [Test]
        public void testMakeQuery()
        {
            using (StreamReader sr = new StreamReader(ConnectionManager.getInstance().makeQuery(
                _token, "901", new Func<String, IList<Clinic>>(new SchedulingDao(null).getClinics), new object[] { "DERM" })))
            {
                String serialized = sr.ReadToEnd();
                IList<Clinic> deserialized = SerializerUtils.deserialize<IList<Clinic>>(serialized);
                Assert.IsTrue(deserialized.Count > 10);
            }
        }
    }
}
