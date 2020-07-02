using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Clinic : HospitalLocation
    {
        public IList<String> specialInstructions;
        public IList<Person> privilegedUsers;

        public string startTime;
        public string appointmentLength;
        public string displayIncrementsPerHour;

        public Clinic() { }
    }
}