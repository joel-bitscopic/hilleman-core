using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class SpecimenType : BaseClass
    {
        public SpecimenType() { }

        public String name;
        public String description;
        public String abbreviation;
        public String snomed;
    }
}