using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class AppointmentType : BaseClass
    {
        public String name;
        public bool inactive;
        public String synonym;
        public bool ignoreMeansTestBilling;

        public AppointmentType() { }
    }
}