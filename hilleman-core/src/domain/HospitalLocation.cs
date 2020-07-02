using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class HospitalLocation : Location
    {
        public String roomBed;
        public String ward;
        public string stopCode { get; set; }

        public HospitalLocation() : base() { }
    }
}