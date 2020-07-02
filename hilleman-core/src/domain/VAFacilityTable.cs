using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class VAFacilityTable : FacilityTable
    {
        public VAFacilityTable() : base() { }

        public new List<VAFacility> facilities;
        public new Dictionary<String, VAFacility> facilitiesById;
        public new Dictionary<String, VAFacility> facilitiesByVISN;
    }
}