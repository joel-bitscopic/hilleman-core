using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class LabPendingOrder
    {
        public LabPendingOrder() { }

        public String ien;
        public Patient patient;
        public Facility orderingSite;
        public Facility collectingSite;
        public String orderingSiteUID;
        public String orderingSiteAccessionNumber;
        public SpecimenType specimenType;
        public DateTime ordered;
        public DateTime collected;
        public DateTime shipped;
        public DateTime received;
        public DateTime completed;
        public String incomingMessageNumber;
        public String shippingManifest;
        public String comments;
        public List<LabPendingOrderTest> orderedTests;
        public Person orderedBy;
        public String localAccessionNumber;

        public LabPendingOrderSource source;
    }

    public enum LabPendingOrderSource
    {
        VISTA_LEDI = 1,
        SQL_EXTRACT_CACHE = 2
    }
}