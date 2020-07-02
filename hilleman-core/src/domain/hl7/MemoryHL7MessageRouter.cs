using System;
using System.Collections.Concurrent;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    public class MemoryHL7MessageRouter : IHL7MessageRouter
    {
        ConcurrentQueue<HL7Message> _messageQueue;

        public MemoryHL7MessageRouter()
        {
            _messageQueue = new ConcurrentQueue<HL7Message>();
        }

        public void handleMessage(HL7Message message)
        {
            _messageQueue.Enqueue(message);
            // save to db or whatever for logging
            // dequeue and process message
        }

        public void handleRaw(string rawMessage)
        {
            throw new NotImplementedException();
        }

        public void log(HL7Message receivedMsg, HL7Message ackMsg)
        {
            throw new NotImplementedException();
        }

        void backgroundMessageProcessor()
        {
            HL7Message currentMsg = null;
            while (true)
            {
                if (_messageQueue.TryDequeue(out currentMsg))
                {

                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}