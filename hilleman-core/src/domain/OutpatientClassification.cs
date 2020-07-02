using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class OutpatientClassification : BaseClass
    {
        public OutpatientClassification() { }

        public OutpatientClassificationType type;
        public String outpatientEncounterLink;
        public String value;
    }
}