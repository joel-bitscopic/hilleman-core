using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.refactoring
{
    [TestFixture]
    public class SchedulingDaoTest
    {
        #region Vars, Setup and Teardown
        IVistaConnection _cxn;
        SourceSystemTable _srcTable = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
        SourceSystem _dewdropRpc;
        SourceSystem _localRpcSource;
        SourceSystem _httpSource;
        SourceSystem _mySrc;
        IVistaConnection _myCxn;

        [OneTimeSetUp]
        public void testFixtureSetUp()
        {
            _httpSource = _srcTable.getSourceSystem("100");
            _dewdropRpc = _srcTable.getSourceSystem("101");
            _localRpcSource = _srcTable.getSourceSystem("901");

            //_cxn = TestHelper.getConnectionFromConnectionPool("101"); // new VistaRpcConnection(_dewdropRpc);
            //_cxn = new VistaHttpRestConnection(_httpSource);


            connectAndLogin();

            _myCxn = new com.bitscopic.hilleman.core.dao.vista.sql.sqlite.VistaSqliteCacheConnection(_mySrc);
        }

        [OneTimeTearDown]
        public void testFixtureTearDown()
        {
            TestHelper.cleanupAfterAllTests(); // no cost to call this if connection pool wasn't setup

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
                if (_cxn.getSource().id == "901")
                {
                    _cxn.connect();
                    new VistaRpcCrrudDao(_cxn).login(new VistaRpcLoginCredentials() { username = "01vehu", password = "vehu01" });
                }
                else if (_cxn.getSource().id == "101")
                {
                    _cxn.connect();
                    new VistaRpcCrrudDao(_cxn).login(new VistaRpcLoginCredentials() { username = "worldvista6", password = "$#happy7" });
                }
            }
        }

        #endregion

      //  [Test]
        public void testDecreaseClinicAvailability()
        {
            connectAndLogin();
            SchedulingDao s = new SchedulingDao(_cxn);
            DateTime appointment = new DateTime(2015, 1, 13, 8, 0, 0); // 1/13/2015 @ 8:00 AM

            s.decreaseClinicAvailability("12", appointment);
        }

        [Test]
        public void testGetUpdatedAvailabilityString()
        {
            Clinic c = new Clinic() { appointmentLength = "30", startTime = "7" };
            DateTime newAppointmentDateTime1 = new DateTime(2015, 1, 13, 10, 0, 0);
            DateTime newAppointmentDateTime2 = new DateTime(2015, 1, 13, 11, 0, 0);
            DateTime newAppointmentDateTime3 = new DateTime(2015, 1, 13, 7, 0, 0);
            DateTime newAppointmentDateTime4 = new DateTime(2015, 1, 13, 7, 30, 0);
            DateTime newAppointmentDateTime5 = new DateTime(2015, 1, 13, 8, 0, 0);
            String mockString = "WE 21  [k] [X] [z] [1] [1] [1] [1] [1]";

            String newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime1, mockString);
            Assert.AreEqual("WE 21  [k] [X] [z] [1] [1] [1] [0] [1]", newString);

            newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime1, newString);
            Assert.AreEqual("WE 21  [k] [X] [z] [1] [1] [1] [A] [1]", newString);

            newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime1, newString);
            Assert.AreEqual("WE 21  [k] [X] [z] [1] [1] [1] [B] [1]", newString);

            // note appointment arg - changing to 7:00 AM
            newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime3, newString);
            Assert.AreEqual("WE 21  [j] [X] [z] [1] [1] [1] [B] [1]", newString);

            newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime3, newString);
            Assert.AreEqual("WE 21  [9] [X] [z] [1] [1] [1] [B] [1]", newString);

            try
            {
                newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime2, mockString);
                Assert.Fail("Should've thrown exception since 11:00 is not available in mock string for clinic params...");
            }
            catch (com.bitscopic.hilleman.core.domain.exception.HillemanBaseException) { /* ok! */ }
            catch (Exception) { throw; } // not ok...

            try
            {
                newString = new SchedulingDao(_cxn).changeAvailabilityString(-1, c, newAppointmentDateTime4, mockString);
                Assert.Fail("Should've thrown exception X is 'smallest' flag value");
            }
            catch (ArgumentException) { /* ok! */ }
            catch (Exception) { throw; } // not ok...

            try
            {
                newString = new SchedulingDao(_cxn).changeAvailabilityString(1, c, newAppointmentDateTime5, mockString);
                Assert.Fail("Should've thrown exception z is 'largest' flag value");
            }
            catch (ArgumentException) { /* ok! */ }
            catch (Exception) { throw; } // not ok...
        }

        [Test]
        public void testGetClinicAvailabilityForDateRange()
        {
            IList<TimeSlot> result = new SchedulingDao(_cxn).getClinicAvailability("64", DateTime.Now.AddDays(-15), DateTime.Now.AddDays(15));

            Assert.IsTrue(result.Count > 5);
        }

        [Test]
        public void testGetClinicAvailability()
        {
            new SchedulingDao(_cxn).getClinicAvailability("12", DateTime.Now);
        }

      //  [Test]
        public void testCreateAppt()
        {
            IList<TimeSlot> availability = new SchedulingDao(_cxn).getClinicAvailability("11", DateTime.Now.AddDays(3));
            
            Appointment newAppt = new Appointment()
            {
                created = new DateTime(2015, 1, 13, 11, 19, 10),
                createdBy = new Person() { id = "95" },
                location = new Location() { id = "11" },
                patient = new Patient() { id = "91" },
                start = availability[0].start, // new DateTime(2015, 1, 13, 11, 15, 0), // needs to have this slot in 44.001!!!
                length = "30",
                status = "NT",
                type = "9",
                purpose = "3" // SCHEDULED VISIT
            };

            new SchedulingDao(_cxn).createAppointment(newAppt.patient.id, newAppt);
        }

      //  [Test]
        public void testCreatePatientAppointment()
        {
            new SchedulingDao(_cxn).createPatientAppointment(
                new Appointment()
                {
                    created = new DateTime(2015, 1, 13, 11, 19, 10),
                    createdBy = new Person() { id = "983" },
                    location = new Location() { id = "8" },
                    patient = new Patient() { id = "10" },
                    start = new DateTime(2015, 1, 13, 11, 15, 0), // needs to have this slot in 44.001!!!
                    status = "NT",
                    type = "9",
                    purpose = "3" // SCHEDULED VISIT
                });
        }

      //  [Test]
        public void testCreateClinicAppointment()
        {
            new SchedulingDao(_cxn).createClinicAppointment(
                new Appointment()
                {
                    created = new DateTime(2015, 7, 16, 9, 8, 48),
                    createdBy = new Person() { id = "983" },
                    length = "30",
                    location = new Location() { id = "12" },
                    patient = new Patient() { id = "24" },
                    start = new DateTime(2015, 3, 16, 13, 0, 0) // needs to have this slot in 44.001!!!
                });
        }

     //   [Test]
        public void testCreateNewAppointmentSlotIfNeeded()
        {
            DateTime theDayAndSlot = new DateTime(2015, 3, 20, 7, 30, 0);
            String clinicId = "12";

            IList<TimeSlot> slots = new SchedulingDao(_cxn).getClinicAvailability(clinicId, theDayAndSlot);
            if (slots != null && slots.Count > 0) // make sure there is availability on this day!
            {
                new SchedulingDao(_cxn).createNewClinicAppointmentSlotIfNeeded(
                    new Appointment()
                    {
                        location = new Location() { id = clinicId },
                        start = theDayAndSlot
                    });
            }
        }

        [Test]
        public void testGetSpecialInstructions()
        {
            IList<String> result = new SchedulingDao(_cxn).getClinicSpecialInstructions("12");
            Assert.AreEqual(result[0], "PROVIDER APPROVAL FOR OB");
        }

        [Test]
        public void testIsPrivilegedUser()
        {
            Assert.IsFalse(new SchedulingDao(_cxn).isPrivilegedUser("12", "4"));
            Assert.IsTrue(new SchedulingDao(_cxn).isPrivilegedUser("12", "8"));
        }

        [Test]
        public void testGetPrivilegedUsers()
        {
            IList<Person> result = new SchedulingDao(_cxn).getPrivilegedUsers("12");
            Assert.AreEqual(result.Count, 4);

            if (result.Count > 0)
            {
                foreach (Person p in result)
                {
                    System.Console.WriteLine(String.Format("User {0} - {1} has access to this clinic", p.idSet.ids[0].id, p.nameString));
                }
            }
        }

        [Test]
        public void testMatchAppointment()
        {
            Appointment result = new SchedulingDao(_myCxn).matchAppointment("6861", "7127820", "3150721.09");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.patient.id == "7127820");
        }

        [Test]
        public void testGetPatientAppts()
        {
            IList<Appointment> result = new SchedulingDao(_cxn).
                getPatientAppointments("80", new DateTime(2001, 1, 1), new DateTime(2019, 12, 31));

            Assert.IsNotNull(result);
        }

        [Test]
        public void testGetAppointmentsByClinic()
        {
            IList<Appointment> appts = new SchedulingDao(_myCxn).getClinicAppointments("4", "3150715", "3150731");

            Assert.IsNotNull(appts);
            Assert.IsTrue(appts.Count > 0);

            foreach (Appointment appt in appts)
            {
                System.Console.WriteLine(SerializerUtils.serialize(appt));
            }

        }

        [Test]
        public void testGetAppointmentsByClinicIsoDate()
        {
            IList<Appointment> appts = new SchedulingDao(_myCxn).getClinicAppointments("12", "2015-03-16", "2015-03-22");

            Assert.IsNotNull(appts);

            foreach (Appointment appt in appts)
            {
                System.Console.WriteLine(SerializerUtils.serialize(appt));
            }
        }

        [Test]
        public void testGetClinicDetails()
        {
            Clinic location = new SchedulingDao(_cxn).getClinicDetails("12");
            Assert.IsFalse(String.IsNullOrEmpty(location.appointmentLength));
        }

        [Test]
        public void testGetClinics()
        {
            IList<Clinic> result = new SchedulingDao(_cxn).getClinics("");
            Assert.IsTrue(result.Count > 5);
            Assert.IsFalse(String.IsNullOrEmpty(result[0].id));
            Assert.IsFalse(String.IsNullOrEmpty(result[0].name));
        }

        [Test]
        public void testGetCancellationReasons()
        {
            IList<CancellationReason> result = new SchedulingDao(_cxn).getCancellationReasons();

            Assert.IsTrue(result.Count > 0);

            TestHelper.cleanupAfterAllTests();
        }

        [Test]
        public void testGetApptStates()
        {
            IList<AppointmentStatus> result = new SchedulingDao(_cxn).getAppointmentStatuses();
            Assert.IsTrue(result.Count > 0);

            TestHelper.cleanupAfterAllTests();
        }

        [Test]
        public void testGetPatientAppt()
        {
            Appointment result = new SchedulingDao(_cxn)
                .getPatientAppointment("86", new DateTime(2015, 3, 4, 10, 0, 0));

            Assert.IsTrue(result.status == "NT");
            Assert.IsTrue(result.location.id == "");

            TestHelper.cleanupAfterAllTests();
        }

        [Test]
        public void testGetClinicApptTimes()
        {
            IList<String> apptsForDay = new SchedulingDao(_cxn)
                .getClinicAppointmentTimes("13", "2015-06-01T23:59:59", "2015-06-03T00:00:01");

            Assert.IsTrue(apptsForDay.Count > 0);
        }

        [Test]
        public void testGetOverbooksForDay()
        {
            Int32 overbooks = new SchedulingDao(_cxn)
                .getOverbooksForDay("13", "2015-06-02");

            Assert.AreEqual(0, overbooks);
        }

       // [Test]
        public void testDeleteClinicApptSlot()
        {
            IVistaConnection cxn = TestHelper.getConnectionFromConnectionPool("101");
            String clinicId = "13";
            DateTime apptDateTime = new DateTime(2099, 12, 31, 16, 20, 0);
            //new ToolsDaoFactory().getToolsDao(cxn).run(String.Format("S ^SC({0},\"S\",{1},0)=\"^44.001DA^^0\"", clinicId, DateUtils.toVistaDateShortTime(apptDateTime)));

            new SchedulingDao(cxn).deleteAppointmentSlot(clinicId, apptDateTime);
        }

       // [Test]
        public void testRemoveOverbook()
        {
            IVistaConnection cxn = TestHelper.getConnectionFromConnectionPool("101");
            new SchedulingDao(cxn).removeOverbook("13", new DateTime(2015, 5, 19, 11, 0, 0), "1");
        }
    }
}
