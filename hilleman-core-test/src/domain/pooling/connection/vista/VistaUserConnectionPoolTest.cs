using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.refactoring;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    [TestFixture]
    public class VistaUserConnectionPoolTest
    {
        [Test]
        public void testPool()
        {
            VistaUserConnectionPool pool = VistaUserConnectionPool.getInstance();

            User user1 = new User();
            user1.id = "joel@bitscopic.com";
            user1.nameString = "MEWTON,JOEL";
            user1.sourceSystemId = "902"; // set this for site to visit
            user1.idSet = new IdentifierSet();
            user1.idSet.add("666111234", "SSN"); // set user's SSN
            user1.idSet.add(new Identifier() { id = "1", sourceSystemId = "901", name = "DUZ" }); // be sure to set correct home site/DUZ

            VistaUserRpcConnection cxn = (VistaUserRpcConnection)pool.checkOutAlive(user1);

            Assert.IsTrue(cxn.IsConnected);
            Assert.IsFalse(cxn.isAvailable);

            try
            {
                pool.checkOutAlive(user1);
            }
            catch (ArgumentException ae)
            {
                Assert.AreEqual(ae.Message, "That user's connection is already in use! It must be returned to the pool before it can be re-used");
            }

            VistaUserConnectionPool.getInstance().checkIn(cxn);

            Assert.IsTrue(cxn.isAvailable); // we still have reference to cxn so show it's available for use again now

            VistaUserRpcConnection cxn2 = (VistaUserRpcConnection)pool.checkOutAlive(user1);
            Assert.IsTrue(cxn2.IsConnected);
            Assert.IsFalse(cxn2.isAvailable);

            Dictionary<String, Accession> accessions = new LabsDao(cxn2).getAccessions();

            VistaUserConnectionPool.getInstance().checkIn(cxn2);

            pool.shutdown();
            System.Threading.Thread.Sleep(5000);
        }
    }
}
