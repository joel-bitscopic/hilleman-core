using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class SystemDaoTest
    {
        #region Setup/Teardown
        IVistaConnection _cxnToReturn;

        [TearDown]
        public void td()
        {
            if (_cxnToReturn != null)
            {
                TestHelper.returnConnection(_cxnToReturn);
            }
        }

        [OneTimeTearDown]
        public void tftd()
        {
            TestHelper.cleanupAfterAllTests();
        }
        #endregion

        [Test]
        public void testSearchRpc()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            IList<RemoteProcedure> result = new SystemDao(_cxnToReturn).searchRpc("ORWPT SELE");
            Assert.AreEqual("ORWPT SELECT", result[0].name);
            Assert.AreEqual(44, result.Count);
        }

        [Test]
        public void testGetRpcsInOption()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            Dictionary<String, MenuOption> rpcOptions = new SystemDao(_cxnToReturn).getRpcOptions();
            KeyValuePair<String, MenuOption> selectedOption = rpcOptions.First(s => s.Value.name == "OR CPRS GUI CHART");
            IList<RemoteProcedure> rpcsInOption = new SystemDao(_cxnToReturn).getRpcsForOption(selectedOption.Key);
            Assert.IsTrue(rpcOptions.Count > 0);
            Assert.IsNotNull(rpcsInOption.First(r => r.name == "ORWPT SELECT"));
        }

        [Test]
        public void testGetVistaMenuOptions()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            Dictionary<String, MenuOption> options = new SystemDao(_cxnToReturn).getVistaMenuOptions();

            Assert.IsTrue(options.Count > 10000, "There should be over 10K options in test VistA!");
        }

        [Test]
        public void testGetRpcDetail()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            IList<RemoteProcedure> rpcSearch = new SystemDao(_cxnToReturn).searchRpc("ORWPT SELE");
            RemoteProcedure rpc = new SystemDao(_cxnToReturn).getRpcDetail(rpcSearch[0].id);

            Assert.IsNotNull(rpc.name);
            Assert.IsNotNull(rpc.description);
        }
    }
}
