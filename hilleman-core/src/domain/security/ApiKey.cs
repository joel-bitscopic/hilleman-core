using System;

namespace com.bitscopic.hilleman.core.domain.security
{
    [Serializable]
    public class ApiKey
    {
        public ApiKey() { }

        public String id;
        public String key;
        public String appName;
        public DateTime issued;
        public DateTime expires;
        public bool active;
        public String contact;
        /// <summary>
        /// Currently just dumping this here as a string - will probably be some sort of JSON in the future, if used
        /// </summary>
        public String perms;
    }
}