using com.bitscopic.hilleman.core.domain.exception;
using System;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    [Serializable]
    public class HL7Exception : HillemanBaseException
    {
        public HL7Exception() : base() { }

        public HL7Exception(String message) : base(message) { }
    }
}