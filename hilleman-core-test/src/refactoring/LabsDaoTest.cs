using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.pooling.connection.vista;
using com.bitscopic.hilleman.core.utils;
using System.Linq;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class LabsDaoTest
    {
        LabsDao _dao;

        [OneTimeSetUp]
        public void testFixtureSetUp()
        {
            _dao = new LabsDao(TestHelper.getConnectionFromConnectionPool("901"));
        }

        [OneTimeTearDown]
        public void testFixtureTearDown()
        {
            VistaRpcConnectionPools.getInstance().shutdown();
        }

        [Test]
        public void testGetSpecimenTypes()
        {
            List<SpecimenType> result = _dao.getSpecimenTypes();
            IEnumerable<SpecimenType> sortedList = result.OrderBy(st => st.name);
            foreach (SpecimenType st in sortedList)
            {
                System.Console.WriteLine(String.Format("{0}^{1}", st.name, st.id));
            }
        }

        [Test]
        public void testGetIENForUID()
        {
            String ien = _dao.get69x6IENFromRemoteUID("5317000012");
        }

        [Test]
        public void testPendingLabOrder()
        {
            String ien = _dao.get69x6IENFromRemoteUID("5317000012");
            LabPendingOrder lpo = _dao.getLabPendingOrder(ien);

            Assert.AreEqual(lpo.orderingSite.id, "671");
        }

        [Test]
        public void testGetReferralPatient()
        {
            Patient p = _dao.getReferralPatient("176263");
            Assert.AreEqual(p.idSet.getByName("LRDFN").id, "408237");
            Assert.AreEqual(p.dateOfBirth, DateUtils.toDateTime("2870605", TimeZoneInfo.Utc));
        }

        [Test]
        public void testGetAccessionAreaIenByAbbreviation()
        {
            Accession result = _dao.getAccessionByAbbreviation("MI");
            Assert.AreEqual(result.id, "12", "12 is the MI accession abbreviation IEN in the 901 test VistA system");
        }

        [Test]
        //[ExpectedException(typeof(ArgumentException), ExpectedMessage = "FAKE not found!")]
        public void testGetAccessionAreaIenByAbbreviationInvalidAbbreviation()
        {
            Accession result = _dao.getAccessionByAbbreviation("FAKE");
        }

        [Test]
        public void testGetAccession()
        {
            Object result = _dao.getTestByAccession("R/VAO 16 5441");
            Assert.IsNotNull(result);

            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented));
        }

        [Test]
        public void testGetLRDFNFromSSN()
        {
            String ssn = "666999911";
            Assert.IsFalse(String.IsNullOrEmpty(_dao.getLRDFNFromSSN(ssn)));
        }

        [Test]
        public void testGetChemLabsForPatient()
        {
            String lrdfn = "360";
            IList<LabTest> result = _dao.getChemLabsForPatient(lrdfn);
        }


    }
}
