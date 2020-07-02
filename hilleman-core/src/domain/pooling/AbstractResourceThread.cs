using System;
using System.Threading;

namespace com.bitscopic.hilleman.core.domain.pooling
{
    public class AbstractResourceThread
    {
        public Thread Thread { get; set; }

        public DateTime Timestamp { get; set; }

        public AbstractPoolSource Source { get; set; }
    }
}
