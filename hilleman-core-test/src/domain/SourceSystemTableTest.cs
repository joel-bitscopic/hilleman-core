using System;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.domain
{
    public class PraedicoSourceSystem
    {
        public PraedicoSourceSystem() { }

        public String id;
        public String text;
    }

    [TestFixture]
    public class SourceSystemTableTest
    {
        [Test]
        public void testPrintSites()
        {
            SourceSystemTable table = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
            foreach (SourceSystem ss in table.sources)
            {
                Assert.That(!String.IsNullOrEmpty(ss.id));
                Assert.That(!String.IsNullOrEmpty(ss.name));
              //  System.Console.WriteLine(ss.id + "_" + ss.name.Replace(" ", "_").Replace(",", ""));
            }

        }


        
    }
}
