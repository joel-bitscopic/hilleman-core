using System;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class ConsultDaoTest
    {
        [OneTimeTearDown]
        public void testFixtureTearDown()
        {
            TestHelper.cleanupAfterAllTests();
        }

        [Test]
        public void testGetConsult()
        {
            Consult result = new ConsultDao(TestHelper.getConnectionFromConnectionPool("101")).getConsult("10");

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.patient);
            Assert.IsNotNull(result.order);
            Assert.IsNotNull(result.order.status);
            Assert.IsFalse(new DateTime().CompareTo(result.entryDate) == 0);
        }
    }
}
