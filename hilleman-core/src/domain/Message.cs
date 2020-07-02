using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Message
    {
        public String from;
        public String to;
        public String subject;
        public String body;
        public String uniqueId;
        public DateTime timestamp;

        public Message() { }
    }
}