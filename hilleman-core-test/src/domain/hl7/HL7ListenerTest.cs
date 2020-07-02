using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    [TestFixture]
    public class HL7ListenerTest
    {
        HL7Listener startHL7Listener()
        {
            HL7Listener listener = new HL7Listener("127.0.0.1", 9999);
            listener.start();
            return listener;
        }

        void stopHL7Listener(HL7Listener listener)
        {
            listener.stop();
        }

        [Test]
        public void testBytesAndChars()
        {
            System.Console.WriteLine((Int32)Char.Parse("\x1C"));
            System.Console.WriteLine((Int32)Char.Parse("\x0D"));
            System.Console.WriteLine((Int32)Char.Parse("\x0B"));
            //Assert.AreEqual(0, System.Text.Encoding.UTF8.GetBytes(new char[] { '\x1C' })[0] & )

            DateTime parsedDOB = DateTime.Now;
            DateTime.TryParseExact("19780511", "yyyyMMdd", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out parsedDOB);

            System.Console.WriteLine(parsedDOB);

            String specimenCollectionDateStr = "20171121131027-0600";
            DateTime parsedSpecimenCollectionDate = default(DateTime);
            DateTime.TryParseExact(specimenCollectionDateStr, "yyyyMMddHHmmsszzz", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out parsedSpecimenCollectionDate);
            System.Console.WriteLine(parsedSpecimenCollectionDate);
        }

        [Test]
        public void testSendHL7Message()
        {
            MyConfigurationManager.getValue("foo");
            MyConfigurationManager.setValue("HL7_MESSAGE_ROUTER_TYPE", "test"); // set the HL7 message router to 'test' to avoid logging messages and actually doing anything with them

            String hl7ServerHostName = "127.0.0.1";
            Int32 hl7ServerPort = 999;
//            HL7Listener listener = startHL7Listener();

            ConcurrentBag<double> totalConnectTime = new ConcurrentBag<double>();
            ConcurrentBag<double> totalSendMsgTime = new ConcurrentBag<double>();


            List<Task> threadedClients = new List<Task>();
            Int32 numThreads = 8;
            for (int i = 0; i < numThreads; i++)
            {
                Task t = new Task(() => sendCannedHL7MessageToServer(hl7ServerHostName, hl7ServerPort, totalConnectTime, totalSendMsgTime));
                t.Start();
                threadedClients.Add(t);
            }


            Int32 numOfSuccessfulMsgs = 0;
            DateTime start = DateTime.Now;
            while (true)
            {
                for (int i = 0; i < threadedClients.Count; i++)
                {
                    Task t = threadedClients[i];
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        numOfSuccessfulMsgs++;
                        threadedClients[i] = new Task(() => sendCannedHL7MessageToServer(hl7ServerHostName, hl7ServerPort, totalConnectTime, totalSendMsgTime));
                        threadedClients[i].Start();
                    }
                    else if (t.Status == TaskStatus.Faulted)
                    {
                        System.Console.WriteLine("Task faulted: " + t.Exception.InnerException.Message);
                        threadedClients[i] = new Task(() => sendCannedHL7MessageToServer(hl7ServerHostName, hl7ServerPort, totalConnectTime, totalSendMsgTime));
                        threadedClients[i].Start();
                    }
                    else
                    {
                        System.Console.WriteLine("Thread " + i.ToString() + " still running...");
                    }
                }

                if (DateTime.Now.Subtract(start).TotalSeconds > 10)
                {
                    break;
                }
            }

            System.Console.WriteLine("Successfully sent " + numOfSuccessfulMsgs.ToString() + " messages in " + DateTime.Now.Subtract(start).TotalSeconds.ToString());
            System.Console.WriteLine("Total time spent on 'connect' operation (ms): " + totalConnectTime.Sum());
            System.Console.WriteLine("Total time spent on 'sending' operation (ms): " + totalSendMsgTime.Sum());

         //   stopHL7Listener(listener);
        }

        void sendCannedHL7MessageToServer(String hostname, int port, ConcurrentBag<double> totalConnectTime, ConcurrentBag<double> totalSendMsgTime)
        {
            String rawHl7Msg = "MSH|^~\\&|SENDING_APPLICATION|SENDING_FACILITY|RECEIVING_APPLICATION|RECEIVING_FACILITY|20110613061611||SIU^S12|24916560|P|2.3||||||\x0D" +
                                "SCH|10345^10345|2196178^2196178|||10345|OFFICE^Office visit|reason for the appointment|OFFICE|60|m|^^60^20110617084500^20110617093000|||||9^DENT^ARTHUR^||||9^DENT^ARTHUR^|||||Scheduled\x0D" +
                                "PID|1||42||BEEBLEBROX^ZAPHOD||19781012|M|||1 Heart of Gold ave^^Fort Wayne^IN^46804||(260)555-1234|||S||999999999|||||||||||||||||||||\x0D" +
                                "PV1|1|O|||||1^Adams^Douglas^A^MD^^^^|2^Colfer^Eoin^D^MD^^^^||||||||||||||||||||||||||||||||||||||||||99158||\x0D" +
                                "RGS|1|A\x0D" +
                                "AIG|1|A|1^Adams, Douglas|D^^\x0D" +
                                "AIL|1|A|OFFICE^^^OFFICE|^Main Office||20110614084500|||45|m^Minutes||Scheduled\x0D" +
                                "AIP|1|A|1^Adams^Douglas^A^MD^^^^|D^Adams, Douglas||20110614084500|||45|m^Minutes||Scheduled";

            HL7Message msgToSend = new HL7Message(rawHl7Msg);
            Assert.AreEqual(msgToSend.segments.Count, 8);
            Assert.AreEqual(msgToSend.getMSH().fieldSeparator, '|');


            DateTime startConnect = DateTime.Now;
            TcpClient client = new TcpClient();
            client.Connect(hostname, port);
            totalConnectTime.Add(DateTime.Now.Subtract(startConnect).TotalMilliseconds);

            DateTime startSendMsg = DateTime.Now;
            byte[] fullEncodedMsg = System.Text.Encoding.UTF8.GetBytes(msgToSend.toEncodedMessage());
            client.GetStream().Write(fullEncodedMsg, 0, fullEncodedMsg.Length);

            byte[] responseBuf = new byte[8192];
            int bytesRead = client.GetStream().Read(responseBuf, 0, responseBuf.Length);
            totalSendMsgTime.Add(DateTime.Now.Subtract(startSendMsg).TotalMilliseconds);

            HL7Message responseFromServer = new HL7Message(System.Text.Encoding.UTF8.GetString(responseBuf, 0, bytesRead));
            client.Close();

            // assert we received a HL7 response with MSH & MSA
            Assert.IsNotNull(responseFromServer.getMSH(), "Missing MSH from response");
            Assert.AreEqual(responseFromServer.segments.Count, 2, "Expected MSH & MSA segment only in response but received " + responseFromServer.segments.Count.ToString() + " segments");
            Assert.IsTrue(responseFromServer.segments.Any(seg => seg.segmentId == "MSA"), "Missing MSA ack from response");

        }
    }
}
