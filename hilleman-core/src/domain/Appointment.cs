using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Appointment : BaseClass
    {
        public Patient patient;
        public DateTime start;
        public DateTime end;
        public DateTime lastUpdated;
        public DateTime created;
        public Location location;
        public String status;
        public String purpose;
        public String type;
        public String length;
        public Person createdBy;
        public String consultLink;
        public String encounterLink;
        public bool areMoreApptsInClinicSlot;
        public bool isOverbook;

        public Appointment() { }
    }
}