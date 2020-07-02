using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class OutpatientEncounter : BaseClass
    {
        public OutpatientEncounter() { }

        public DateTime date;
        public Patient patient;
        public HospitalLocation location;
        public Visit visit;
        public AppointmentType appointmentType;

        public IList<OutpatientClassification> classifications;
    }
}