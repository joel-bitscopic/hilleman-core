using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using com.bitscopic.hilleman.core.utils;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    /// <summary>
    /// Wrapper for async Listener callbacks, main thread threading handlers, shutdown flag
    /// </summary>
    public class ListenerState
    {
        public Socket socket;
        public ManualResetEvent resetEvent;
        public bool shutdown;

        public ListenerState()
        {
            this.resetEvent = new ManualResetEvent(false);
        }
    }

    /// <summary>
    /// State object for the Listener's receiver
    /// </summary>
    public class ListenerReceiverState
    {
        public Socket worker;
        public byte[] buffer;
        public Int64 bytesRead;
        public MemoryStream bufferStream;
        public List<String> rawHL7Messages;
        public Int32 bufferSize = 4096;
        public IPEndPoint remoteEndPoint;
    }

    /// <summary>
    /// High performance/multi-threaded generic TCP/IP server with a couple HL7 specific bits for message routing
    /// </summary>
    public class HL7Listener
    {
        ListenerState _state = new ListenerState();
        String _hostname;
        Int32 _port;

        public HL7Listener(String hostname, Int32 port)
        {
            _hostname = hostname;
            _port = port;
        }

        public void start()
        {
            Thread t = new Thread(new ThreadStart(this.startSync));
            t.Start();
        }

        /// <summary>
        /// Main thread for the main server/socket listener
        /// </summary>
        public void startSync()
        {
            IPAddress[] ipsForHost = Dns.GetHostAddresses(_hostname);
            IPEndPoint endpoint = null;
            if (ipsForHost == null || ipsForHost.Length == 0)
            {
                throw new ArgumentException("No IP addresses found for hostname");
            }
            foreach (IPAddress ip in ipsForHost)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    endpoint = new IPEndPoint(ip, _port);
                }
            }

            if (endpoint == null)
            {
                throw new Exception("Unable to determine endpoint for socket");
            }

            try
            {
                _state.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _state.socket.Bind(endpoint);
                _state.socket.Listen(256); // start listener!
                _state.socket.BeginAccept(new AsyncCallback(socketAccept), _state);
            }
            catch (SocketException e)
            {
                _state.socket.Close();
            }
            catch (Exception) { /* we don't care about socket close errors! */ }

            while (!_state.shutdown)
            {
                _state.resetEvent.Reset();
                _state.resetEvent.WaitOne();
            }

            try
            {
                _state.socket.Close();
            }
            catch (Exception) { /* we don't care about socket close errors! */ }
        }

        public void stop()
        {
            _state.shutdown = true;
            _state.resetEvent.Set();
        }

        void socketAccept(IAsyncResult iar)
        {
            ListenerState state = (ListenerState)iar.AsyncState;
            if (state.shutdown) // don't accept new sockets if shutting down!
            {
                return;
            }

            Socket newSocket = state.socket.EndAccept(iar);
            _state.socket.BeginAccept(new AsyncCallback(socketAccept), _state);
            _state.resetEvent.Set();

            ListenerReceiverState receiver = new ListenerReceiverState();
            receiver.buffer = new byte[receiver.bufferSize];
            receiver.worker = newSocket;
            receiver.remoteEndPoint = ((IPEndPoint)newSocket.RemoteEndPoint);

            try
            {
                newSocket.BeginReceive(receiver.buffer, 0, receiver.bufferSize, SocketFlags.None, new AsyncCallback(receivingData), receiver);
            }
            catch (Exception)
            {
                try
                {
                    newSocket.Shutdown(SocketShutdown.Both);
                    newSocket.Close();
                }
                catch (Exception) { }
            }
        }

        void receivingData(IAsyncResult iar)
        {
            char HL7_EOT = HL7Helper.getHL7EOTCharFromConfig(); // end of tx char  
            char HL7_EOT_NM = HL7Helper.getHL7EOT_NMCharFromConfig(); // HL7 messages may be chained: <SOT>hl7 message 1<EOT><EOT_NM><SOT>hl7 message 2<EOT><EOT_NM><SOT>hl7 message 3<EOT> etc...
            char HL7_SOT = HL7Helper.getHL7SOTCharFromConfig();
            char HL7_SEG_DELIM = HL7Helper.getHL7SegmentDelimiterCharFromConfig();

            ListenerReceiverState receiver = (ListenerReceiverState)iar.AsyncState;
            Socket worker = receiver.worker;
            //char eot = HL7_EOT; // HL7 "standard" end-of-tx character
            byte eotByte = Convert.ToByte(HL7_EOT);
            byte eotNextMessageByte = Convert.ToByte(HL7_EOT_NM);
            byte sotByte = Convert.ToByte(HL7_SOT);

            List<HL7Message> receivedMessages = new List<HL7Message>();
            try
            {
                int bytesRead = worker.EndReceive(iar);
                if (bytesRead == 0 && receiver.bytesRead == 0) 
                {
                    worker.Close();
                    return;
                }

                receiver.bufferStream = new MemoryStream();
                bool reachedEot = false;
                byte secondToLastCharRead = System.Text.Encoding.UTF8.GetBytes(new char[] { HL7_SOT })[0];
                byte lastCharRead = System.Text.Encoding.UTF8.GetBytes(new char[] { HL7_SOT })[0];
                while (!reachedEot)
                {
                    if (bytesRead == 0)
                    {
                        reachedEot = true;
                        break;
                    }

                    receiver.bufferStream.Write(receiver.buffer, 0, bytesRead);
                    receiver.bytesRead += bytesRead;

                    // more to read?
                    if (bytesRead == receiver.bufferSize && receiver.worker.Available > 0)
                    {
                        receiver.buffer = new byte[receiver.bufferSize];
                        bytesRead = worker.Receive(receiver.buffer); // again read what's available in to buffer
                        continue;
                    }
                    else
                    {
                        reachedEot = true;
                        continue;
                    }
                }

                // create buffer for message and copy memory stream to it
                receiver.buffer = new byte[receiver.bytesRead];
                receiver.bufferStream.Position = 0;
                receiver.bufferStream.Read(receiver.buffer, 0, Convert.ToInt32(receiver.bytesRead));

                // get all HL7 messages that were received and extract message ID from each
                String allMessagesOneString = System.Text.Encoding.UTF8.GetString(receiver.buffer);
                LogUtils.debug(allMessagesOneString);
                // split up messages if there were more than one - HL7 spec states multiple messages may be sent on same payload - use SOT char for split
                List<String> splitMsgs = StringUtils.splitToList(allMessagesOneString, HL7_SOT, StringSplitOptions.RemoveEmptyEntries);
                // collection for all parsed messages
                List<HL7Message> parsedMsgs = new List<HL7Message>();
                IHL7MessageRouter router = HL7MessageRouterFactory.getMessageRouter();

                foreach (String s in splitMsgs)
                {
                    // HL7
                    String cleaned = s.TrimEnd(HL7_EOT);
                    cleaned = s.TrimEnd(HL7_EOT_NM);
                    cleaned = s.TrimEnd(HL7_EOT);

                    HL7Message parsedMsg = null;
                    try
                    {
                        router.handleRaw(cleaned);
                        parsedMsg = new HL7Message(cleaned);
                        parsedMsg.sentFrom = receiver.remoteEndPoint.Address.ToString();
                        parsedMsgs.Add(parsedMsg);
                        //router.handleMessage(parsedMsg);
                    }
                    catch (Exception parsingExc)
                    {
                        // reply with valid HL7 message AE or AR signifying the received message was invalid
                        HL7Message errResponse = new HL7Message().getUnsolicitedNack("Invalid message. Send only valid HL7 - " + parsingExc.Message);
                        byte[] nackBytes = System.Text.Encoding.UTF8.GetBytes(errResponse.toEncodedMessage());
                        worker.Send(nackBytes);
                        worker.Close();
                        throw;
                    }
                }

                // send ACKs for all received messages
                StringBuilder sb = new StringBuilder();
                foreach (HL7Message receivedMsg in parsedMsgs)
                {
                    HL7Message ack = receivedMsg.getAck();
                    router.handleMessage(receivedMsg);
                    router.log(receivedMsg, ack);
                    sb.Append(ack.toEncodedMessage());
                }

                byte[] ackBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                worker.Send(ackBytes);
                worker.Close();
            }
            catch (Exception)
            {
                // shutdown worker socket on error
                try
                {
                    worker.Shutdown(SocketShutdown.Both);
                    worker.Close();
                }
                catch (Exception) { }
                //throw; // the listener is async - throwing an exception here in this "background" thread handling the request will bring down the process
            }
        }

    }
}
