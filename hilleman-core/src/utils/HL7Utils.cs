using com.bitscopic.hilleman.core.domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace com.bitscopic.hilleman.core.utils
{
    public static class HL7Utils
    {
        private static Int32 serialNumber = 0;
        
        /// <summary>
        /// Fetch a 20 character unique message control ID - first 14 characters are current UTC time string (yyyyMMddHHmmss) concatenated by rolling 6 character serial number (e.g. 000001, 000002, ..., 999999, 000001 etc).
        /// 
        /// The counter resets with the parent process and is not maintained in a separate state anywhere
        /// </summary>
        /// <returns></returns>
        public static string getUniqueMessageControlId()
        {
            Int32 incrementedValue = Interlocked.Increment(ref serialNumber);
            if (incrementedValue == 999999) // control number of characters to 6 per rightPack call below
            {
                serialNumber = 0;
            }
            String datePart = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return String.Concat(datePart, StringUtils.rightPack(incrementedValue.ToString(), 6, '0'));
        }
    }
}