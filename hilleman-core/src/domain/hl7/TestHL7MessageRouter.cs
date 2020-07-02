using System;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    public class TestHL7MessageRouter : IHL7MessageRouter
    {
        Int32 messagesReceived = 0;
        public TestHL7MessageRouter() { }

        public void handleMessage(HL7Message message)
        {
            System.Console.WriteLine("Received HL7! MSH:\r\n" + message.getMSH().toEncodedString(message));
            return;
        }

        public void handleRaw(string rawMessage)
        {
            messagesReceived++;
            if (messagesReceived % 10 == 0)
            {
                System.Console.WriteLine("Received " + messagesReceived + " from clients since start");
            }
            System.Console.WriteLine("Received message:\r\n" + rawMessage);
            return;
        }

        public void log(HL7Message receivedMsg, HL7Message ackMsg)
        {
           // System.Console.WriteLine("TestHL7MessageRouter doesn't log the raw message");
            return;
        }
    }
}
