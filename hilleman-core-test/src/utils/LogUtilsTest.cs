using System;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.utils
{
    [TestFixture]
    public class LogUtilsTest
    {

        public void testLog()
        {
            LogUtils.LOG("You're very good at logging");
        }
    }
}
