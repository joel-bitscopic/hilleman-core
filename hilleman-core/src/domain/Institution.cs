using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Institution : BaseClass
    {
        public Institution() { }

        public String name;
        public String shortName;
        public String typeCode;
        public String stationNumber;
        public String status;
    }
}