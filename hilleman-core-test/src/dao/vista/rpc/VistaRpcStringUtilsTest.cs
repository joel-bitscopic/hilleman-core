using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    [TestFixture]
    public class VistaRpcStringUtilsTest
    {
        [Test]
        public void testConvertListToString()
        {
            Dictionary<String, String> input = new Dictionary<string, string>();

            input.Add("1", "value 1");
            input.Add("2", "value 2");
            input.Add("3", "");


            String result = VistaRpcStringUtils.convertListToString(input);

            Assert.AreEqual("", result);
        }
    }
}
