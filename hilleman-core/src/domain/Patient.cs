using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Patient : Person
    {
        public HospitalLocation hospitalLocation;
        public IList<Appointment> appointments;

        public Patient() { }
    }
}