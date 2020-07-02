using System;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class HL7UtilsTest
    {
        [Test]
        public void testGetUniqueHL7MessageId()
        {
            Int32 numIterations = 100;

            DateTime start = DateTime.Now;
            for (int i = 0; i < numIterations; i++)
            {
                HL7Utils.getUniqueMessageControlId(); // taking < 1 second for 1 million iterations July 10, 2018 
            }

           // System.Console.WriteLine(String.Format("Took {0} seconds for {1} iterations", DateTime.Now.Subtract(start).TotalSeconds.ToString(), numIterations.ToString()));
        }
    }
}
