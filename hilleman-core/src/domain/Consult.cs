using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Consult : BaseClass
    {
        public Consult() { }

        public DateTime entryDate;
        public DateTime requestDate;
        public Patient patient;
        public String displayText;
        public String clinicalProcedureIen;
        public Order order;
    }
}