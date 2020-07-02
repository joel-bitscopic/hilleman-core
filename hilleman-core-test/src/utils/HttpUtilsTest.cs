using System;
using NUnit.Framework;
using Newtonsoft.Json;
using com.bitscopic.hilleman.core.dao;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class HttpUtilsTest
    {
        String _baseUri = "http://192.168.2.110:8081/crudsvc/v0.1/";

        [Test]
        public void testGet()
        {
            String response = HttpUtils.Get(new Uri(_baseUri), "200/.5,");
            Assert.AreEqual(response, "{\"type\":\"ARRAY\",\"value\":[\"[Data]\",\"200^.5^.01^POSTMASTER^POSTMASTER\",\"200^.5^2^;^<Hidden>\",\"200^.5^2.2^;^;\",\"200^.5^9.3^9^C-VT100\",\"200^.5^10.1^1^200\",\"200^.5^20.2^ POSTMASTER^ POSTMASTER\",\"200^.5^30^2960604^JUN 04, 1996\",\"200^.5^31^.5^POSTMASTER\",\"200^.5^41.98^N^NEEDS ENTRY\",\"200^.5^202^3121015.17554^OCT 15, 2012@17:55:40\",\"200^.5^202.03^0^No\",\"200^.5^203.1^56895,57011^56895,57011\",\"200^.5^8932.001^0^0\",\"200^.5^8980.16^.5^\"]}");
            ReadRangeResponse deserialized = JsonConvert.DeserializeObject<ReadRangeResponse>(response);


            Assert.IsNotNull(deserialized);
            Assert.IsTrue(deserialized.value.Count > 0);
        }

        [Test]
        public void testPost()
        {
            String postBody = "{ \"file\" : \"2\", \"fields\" : \".01;.09;991.01\" }";

            String response = HttpUtils.Post(new Uri(_baseUri), "range", postBody);
            ReadRangeResponse deserialized = JsonConvert.DeserializeObject<ReadRangeResponse>(response);
            Assert.AreEqual(deserialized.value.Count, 297);
        }
    }
}
