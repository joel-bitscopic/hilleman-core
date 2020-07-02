using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class LabTestResult
    {
        public LabTestResult() { }

        public String testName;
        public String resultType;
        public String result;
        public String refLow;
        public String refHigh;
        public String units;
        public String interpretation;
        public User enteredBy;
        public DateTime entered;

        public DateTime resultAvailable;
        public SourceSystem originatingSite;
        public SourceSystem collectingSite;
        public SourceSystem completingSite;
        public Specimen specimen;
    }
}