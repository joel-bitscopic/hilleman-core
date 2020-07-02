using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class FacilityTable
    {
        public FacilityTable() { }

        public List<Facility> facilities;
        public Dictionary<String, Facility> facilitiesById;
        public Dictionary<String, Facility> facilitiesByVISN;
    }
}