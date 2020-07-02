using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class AppointmentStatus : BaseClass
    {
        public String name;
        public String abbreviation;
        public bool checkInAllowed;
        public bool checkOutAllowed;
        public bool cancelAllowed;
        public bool noShowAllowed;

        public AppointmentStatus() { }
    }
}