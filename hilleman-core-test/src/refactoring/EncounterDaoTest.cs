using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class EncounterDaoTest
    {
        [Test]
        public void testGetEncounter()
        {
            OutpatientEncounter oe = new EncounterDao(TestHelper.getConnectionFromConnectionPool("901")).getEncounter("6226");

            Assert.IsNotNull(oe);

            Assert.IsNotNull(oe.appointmentType);
            Assert.IsNotNull(oe.patient);
            Assert.IsNotNull(oe.visit);

            new EncounterDao(TestHelper.getConnectionFromConnectionPool("901")).addOutpatientClassifications(oe);

            Assert.IsNotNull(oe.classifications);
            Assert.IsTrue(oe.classifications.Count > 0);
        }

        #region Helper Tests

        [Test]
        public void testGetOutpatientClassificationTypes()
        {
            IList<OutpatientClassificationType> types = new EncounterDao(TestHelper.getConnectionFromConnectionPool("901")).getOutpatientClassificationTypes();
            Assert.IsNotNull(types);
            Assert.IsTrue(types.Count > 0);

            Assert.IsNotNull(BaseClassUtils.matchById<OutpatientClassificationType>(types, "3"));
        }

        #endregion
    }
}
