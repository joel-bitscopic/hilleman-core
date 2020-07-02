using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class OutpatientClassificationType : BaseClass
    {
        public OutpatientClassificationType() { }

        public String name;
        public String prompt;
        public String inputType;
        public String displayName;
        public String abbreviation;
    }
}