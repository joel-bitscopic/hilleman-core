using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class VAFacility : Facility
    {
        public VAFacility() : base() { }

        public String visn;
        public String region;
    }
}