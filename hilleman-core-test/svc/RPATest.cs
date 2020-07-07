using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.svc
{
    [TestFixture]
    public class RPATest
    {
        public void testInvokeRPA()
        {
            Dictionary<String, String> args = new Dictionary<string, string>();
            args.Add("site", "901"); // site 901 is in SourceSystemTable database config 
            args.Add("accessCode", "worldvista6"); 
            args.Add("verifyCode", "$#happy7");
            args.Add("patientSSN", "000001234"); // choose real SSN!

            String postResponse = HttpUtils.Post(
                new Uri("http://127.0.0.1:5000/svc/RPA.svc/"),
                "invokeRPA",
                SerializerUtils.serialize(args));

            List<Patient> responseDeserialized = SerializerUtils.deserialize<List<Patient>>(postResponse);
        }
    }
}
