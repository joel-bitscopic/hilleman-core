using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.session
{
    [Serializable]
    public class HillemanApp
    {
        public HillemanApp() { }

        public String name;
        public String id;
        public String token;
        public DateTime created;
        public DateTime expires;
        public List<User> contacts;
    }
}