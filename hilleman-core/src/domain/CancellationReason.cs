using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class CancellationReason : BaseClass
    {
        public String name;
        public String type;
        public String synonym;
        public bool active = true;

        public CancellationReason()
        {
        }
    }
}