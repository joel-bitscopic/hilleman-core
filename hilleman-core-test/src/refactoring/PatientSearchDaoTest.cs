using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class PatientSearchDaoTest
    {
        IVistaConnection _cxn;

        [OneTimeSetUp]
        public void testFixturSetUp()
        {
            SourceSystemTable srcs = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
            //_cxn = new VistaHttpRestConnection(srcs.getSourceSystem("100"));
            //_cxn = new VistaRpcConnection(srcs.getSourceSystem("101")); // dewdrop RPC
            _cxn = new VistaRpcConnection(srcs.getSourceSystem("901")); // local RPC

            connectAndLogin();
        }

        [OneTimeTearDown]
        public void testFixtureTearDown()
        {
            if (_cxn != null && _cxn.getSource() != null && _cxn.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                if ((_cxn as VistaRpcConnection).IsConnected)
                {
                    _cxn.disconnect();
                }
            }
        }

        void connectAndLogin()
        {
            if (_cxn != null && _cxn.getSource() != null && _cxn.getSource().type == SourceSystemType.VISTA_RPC_BROKER)
            {
                _cxn.connect();
                new VistaRpcCrrudDao(_cxn).login(new VistaRpcLoginCredentials() { username = _cxn.getSource().credentials.username, password = _cxn.getSource().credentials.password }); 
            }
        }

        [Test]
        public void testGetStates()
        {
            ReadRangeRequest request = new ReadRangeRequest(_cxn.getSource());
            request.setFile("5");
            request.setFlags("IP");
            request.setFields(".01");
            request.setCrossRef("#");

            ReadRangeResponse response = new VistaRpcCrrudDao(_cxn).readRange(request);
            System.Console.WriteLine(SerializerUtils.serialize(response));
        }

        [Test]
        public void testSearchLastFourSSN()
        {
            IList<Patient> result = new PatientSearchDao(_cxn).searchForPatient("1234");
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 39); // just how many our GT.M database contains!

            Assert.AreEqual(result[0].firstName, "ALEX");
        }

        [Test]
        public void testSearchLastInitialPlusLastFourSSN()
        {
            IList<Patient> result = new PatientSearchDao(_cxn).searchForPatient("M1234");
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 44); // just how many our GT.M database contains!

            Assert.AreEqual(result[0].firstName, "SCOTT");
        }

        [Test]
        public void testSearchFullSSN()
        {
            IList<Patient> result = new PatientSearchDao(_cxn).searchForPatient("666112222");
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 44);
        }

        [Test]
        public void testBuildPatientSearchRequest()
        {
            ReadRangeRequest request = new PatientSearchDao(_cxn).buildSearchForPatientRequest("ZZ");
            Assert.IsFalse(String.IsNullOrEmpty((String)request.buildRequest()));
        }


        [Test]
        public void testGetPatient()
        {
            Patient p = new PatientSearchDao(_cxn).getPatient("91");
            Assert.IsNotNull(p.idSet);
        }
    }
}
