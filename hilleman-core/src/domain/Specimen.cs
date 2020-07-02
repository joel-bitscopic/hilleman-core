using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Specimen : BaseClass
    {
        public Specimen() { }

        public SpecimenType type;
        public String sourceSite;
        public DateTime collectionDate;
        public Person collectedBy;
        public Accession accession;
    }
}