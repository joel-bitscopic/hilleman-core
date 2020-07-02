using System;
using System.Text;
using com.bitscopic.hilleman.core.utils;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.session
{
    [Serializable]
    public class HillemanRequest
    {
        public HillemanRequest() { }

        public String requestName;
        public object[] args;
        public DateTime requestTimestamp;
        public DateTime responseTimestamp;
        public String serializedRequest;
        public String serializedResponse;

        /// <summary>
        /// Build a string from the args array calling each object's ToString method internally. Args are delimited with UNIT SEPARATOR ascii character
        /// </summary>
        /// <returns></returns>
        internal string getArgsString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.args != null && this.args.Length > 0)
            {
                IList<String> argsAsString = new List<String>();
                foreach (object arg in this.args)
                {
                    argsAsString.Add(arg.ToString());
                }
                sb.Append(StringUtils.join(argsAsString, "\x1e"));
            }
            return sb.ToString();
        }
    }
}