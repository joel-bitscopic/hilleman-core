using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class WaitListEntry : BaseClass
    {
        public WaitListEntry() { }

        /// <summary>
        /// The patient
        /// </summary>
        public Patient patient;

        /// <summary>
        /// Date patient was placed on the wait list
        /// </summary>
        public DateTime originatingDate;

        /// <summary>
        /// The institution (file 4) the patient is on the Wait List
        /// </summary>
        public Institution institution;

        /// <summary>
        /// Current transmission status (0: not transmitted, 1: transmitted)
        /// </summary>
        public String transmissionStatus;

        public DateTime transmissionDate;

        /// <summary>
        /// 1:PCMM TEAM ASSIGNMENT,2:PCMM POSITION ASSIGNMENT,3:SERVICE/SPECIALITY,4:SPECIFIC CLINIC
        /// </summary>
        public String type;

        /// <summary>
        /// Pointer to 405.41
        /// </summary>
        public String teamLink;

        /// <summary>
        /// Pointer to 404.57
        /// 
        /// Enter the position the patient is waiting to be assigned. 
        /// Only active, over capacity positions assigned to the patient's primary care team are selectable.
        /// 
        /// If the patient is waiting for a PCMM Position, the position that the
        /// patient is waiting to be assigned is entered here. The position must be
        /// active, assigned to the team the patient is currently assigned and the
        /// position must be above capacity. 
        /// The patient can have multiple openPosition Wait List assignments.
        /// </summary>
        public String position;

        /// <summary>
        /// Pointer to 409.31
        /// </summary>
        public String serviceSpecialty;

        /// <summary>
        /// Pointer to clinic location
        /// </summary>
        public String clinicLocation;

        public String originatingUser;

        public String priority;

        public String requestedBy;
    }
}