using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class LabPendingOrderTest : BaseClass
    {
        public LabPendingOrderTest() { }

        public String nltId;
        public String nltName;

        public String remoteTestId;
        public String remoteTestName;

        public String localTestId;
        public String localTestName;

        public String outgoingMsgNumber;

        public String localTestUID;

        public SpecimenType specimen;
        public bool updatedTestTypeFlag = false;
        public bool updatedSpecimenTypeFlag = false;

        public LabPendingOrderTest clone()
        {
            return new LabPendingOrderTest()
            {
                id = this.id,
                sourceSystemId = this.sourceSystemId,
                nltId = this.nltId,
                nltName = this.nltName,
                remoteTestId = this.remoteTestId,
                remoteTestName = this.remoteTestName,
                localTestId = this.localTestId,
                localTestName = this.localTestName, 
                outgoingMsgNumber = this.outgoingMsgNumber,
                localTestUID = this.localTestUID,
                specimen = (this.specimen == null ? null : new SpecimenType() { id = this.specimen.id, name = this.specimen.name })
                //,matchedTestType = this.matchedTestType.clone()
            };
        }
    }

}