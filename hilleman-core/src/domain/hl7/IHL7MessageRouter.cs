using System;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    public interface IHL7MessageRouter
    {
        void log(HL7Message receivedMsg, HL7Message ackMsg);
        void handleMessage(HL7Message message);
        void handleRaw(String rawMessage);
    }
}