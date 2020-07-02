using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.svc
{
    [TestFixture]
    public class SchedulingTest
    {
        public void testGetClinicsAsClient()
        {
            String postResponse = HttpUtils.Post(
                new Uri("http://localhost/hilleman/svc/User.svc/"), 
                "authenticate",
                SerializerUtils.serialize(new Credentials() { provider = new SourceSystem() { id = "101" }, username = "01vehu", password = "vehu01" }));

            User loginResult = SerializerUtils.deserialize<User>(postResponse);
            Assert.IsFalse(String.IsNullOrEmpty(loginResult.nameString));
        }

        public void testGetClinicAppointments()
        {
            Uri userUri = new Uri("http://localhost/hilleman/svc/User.svc/");
            Uri schedulingUri = new Uri("http://localhost/hilleman/svc/Scheduling.svc/");

            String postResponse = HttpUtils.Post(
                userUri,
                "authenticate",
                SerializerUtils.serialize(new Credentials() { provider = new SourceSystem() { id = "101" }, username = "worldvista6", password = "$#happy7" }));

            Assert.IsFalse(String.IsNullOrEmpty(HttpUtils.lastRequestHeaders["Access-Control-Session-Token"]));
            User loginResult = SerializerUtils.deserialize<User>(postResponse);
            Assert.IsFalse(String.IsNullOrEmpty(loginResult.nameString));

            Dictionary<String, String> accessControlHeaders = new Dictionary<string, string>();
            accessControlHeaders.Add("Access-Control-Session-Token", HttpUtils.lastRequestHeaders["Access-Control-Session-Token"]);

            IList<Appointment> appts = SerializerUtils.deserialize<IList<Appointment>>(HttpUtils.Get(
                schedulingUri,
                "101/clinic/12/appointments?startDate=2015-03-16&endDate=2015-03-30",
                accessControlHeaders));

            Assert.IsNotNull(appts);
            Assert.IsTrue(appts.Count > 0);

        }


/*        public void testGetClinics()
        {
            UserSvc userSvc = new UserSvc();
            User loggedInUser = new UserMgtDao(null).validateCredentials(
                new Credentials()
                {
                    provider = new SourceSystem() { id = "901", name = "CPM" },
                    username = "01vehu",
                    password = "vehu01"
                });
            String myToken = loggedInUser.token.value;

            Stream s = ConnectionManager.getInstance().makeQuery(myToken, "901", new Func<String, IList<Clinic>>(new SchedulingDao(null).getClinics), new object[] { "A" });
            IList<Clinic> result = SerializerUtils.deserializeFromStream<IList<Clinic>>(s);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 1);
        }
        */
    }
}
