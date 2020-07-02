using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Facility : BaseClass
    {
        public String name;
        public String alternateName;
        public Address address;
        public String type;
        public GeographicCoordinate coordinates;
        public String notes;

        public Facility() { }
    }
}