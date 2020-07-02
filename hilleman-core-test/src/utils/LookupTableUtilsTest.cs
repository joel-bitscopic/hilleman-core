using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class LookupTableUtilsTest
    {
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

        [Test]
        public void testGetLookupTable19x1()
        {
            IVistaConnection cxn = TestHelper.getConnectionFromConnectionPool("901");

            try
            {
                Dictionary<String, String> result = LookupTableUtils.getLookupTable(cxn, "19.1");
                Assert.IsTrue(result.Count > 10, "Should be way more than 10 security keys in 19.1!");

                DateTime benchmarkStart = DateTime.Now;
                for (int i = 0; i < 1000000; i++)
                {
                    result = LookupTableUtils.getLookupTable(cxn, "19.1");
                    Assert.IsTrue(result.Count > 10, "Should be way more than 10 security keys in 19.1!");
                }
                DateTime benchmarkEnd = DateTime.Now;

                Assert.IsTrue(benchmarkEnd.Subtract(benchmarkStart).TotalSeconds < 10, "Subsequent access to the same lookup table should use the cache and be super fast!");
            }
            catch (Exception exc)
            {
                Assert.Fail(exc.Message);
            }
            finally
            {
                TestHelper.returnConnection(cxn);
            }
        }

        [Test]
        //[ExpectedException(typeof(CrrudException), ExpectedMessage = "The input parameter that identifies the file is missing or invalid.")]
        public void testLookupTableFakeFile()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            LookupTableUtils.getLookupTable(_cxnToReturn, "FAKE");
        }

        [Test]
        public void testGetSortedLookupTable()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");
            IList<KeyValuePair<String, String>> result = LookupTableUtils.getSortedLookupTable(_cxnToReturn, "8994");
            for (int i = 0; i < result.Count - 1; i++)
            {
                Assert.IsTrue(0 > result[i].Value.CompareTo(result[i + 1].Value));
            }

            //System.Console.WriteLine("Compared " + result.Count.ToString() + " sorted values!");
        }

        [Test]
        public void testNEntriesFromSortedLookupTable()
        {
            _cxnToReturn = TestHelper.getConnectionFromConnectionPool("901");

            for (int n = 0; n < 10000; n++)
            {
                IList<KeyValuePair<String, String>> result = LookupTableUtils.getNEntriesFromLookupTable(_cxnToReturn, "8994", "ORWPT SELE", 44);

                Assert.IsTrue(result[0].Value == "ORWPT SELECT");
                for (int i = 0; i < result.Count - 1; i++)
                {
                    Assert.IsTrue(0 > result[i].Value.CompareTo(result[i + 1].Value));
                }

            }
            //System.Console.WriteLine("Compared " + result.Count.ToString() + " sorted values!");
        }


        
    }
}
