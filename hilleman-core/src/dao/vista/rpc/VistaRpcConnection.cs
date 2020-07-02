using System;
using com.bitscopic.hilleman.core.domain;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net;
using com.bitscopic.hilleman.core.domain.exception.vista;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcConnection : com.bitscopic.hilleman.core.domain.pooling.AbstractTimedResource, IVistaConnection
    {
        const int CONNECTION_TIMEOUT = 30000;
        const int READ_TIMEOUT = 120000;
        const int DEFAULT_PORT = 9200;
        SourceSystem _source;
        Socket _vistaSocket;
        bool _isConnected;
        bool _isConnecting;
        public bool IsConnected { get { return _isConnected; } }
        public SourceSystem SourceSystem { get { return _source; } }
        public VistaRpcLoginCredentials Credentials;
        public String BrokerContext;

        public VistaRpcConnection(SourceSystem source)
        {
            _source = source;
        }

        public object query(object request)
        {
            if (!_isConnected && !_isConnecting)
            {
                throw new Exception("There is no open socket");
            }

            base.resetTimer(); // reset timer when a call to VistA occurs

            byte[] bytesSent = null;
            if (request is VistaRpcQuery)
            {
                bytesSent = System.Text.Encoding.ASCII.GetBytes(((VistaRpcQuery)request).buildMessage());
            }
            else if (request is String)
            {
                bytesSent = System.Text.Encoding.ASCII.GetBytes((String)request);
            }
            Byte[] bytesReceived = new Byte[256];


            int bytes = 0;
            string reply = "";
            StringBuilder sb = new StringBuilder();
            string thisBatch = "";
            bool isErrorMsg = false;
            int endIdx = -1;

            try
            {
                _vistaSocket.Send(bytesSent, bytesSent.Length, 0);
                bytes = _vistaSocket.Receive(bytesReceived, bytesReceived.Length, 0);
            }
            catch (Exception)
            {
                _isConnected = false;
                throw;
            }

            if (bytes == 0)
            {
                _isConnected = false;
                throw new ZeroBytesReceivedException("Received zero bytes from Vista!");
            }

            thisBatch = Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            endIdx = thisBatch.IndexOf('\x04');
            if (endIdx != -1)
            {
                thisBatch = thisBatch.Substring(0, endIdx);
            }

            if (sb.Length == 0 && !String.IsNullOrEmpty(thisBatch) && thisBatch.Contains("Instance is not running"))
            {
                _isConnected = false;
                throw new Exception("Broker appears to be temporarily down: " + thisBatch);
            }

            if (bytesReceived[0] != 0)
            {
                thisBatch = thisBatch.Substring(1, bytesReceived[0]);
                isErrorMsg = true;
            }
            else if (bytesReceived[1] != 0)
            {
                thisBatch = thisBatch.Substring(2);
                isErrorMsg = true;
            }
            else
            {
                thisBatch = thisBatch.Substring(2);
            }
            sb.Append(thisBatch);

            // now we can start reading from socket in a loop
            MemoryStream ms = new MemoryStream();
            while (endIdx == -1)
            {
                bytes = _vistaSocket.Receive(bytesReceived, bytesReceived.Length, 0);
                if (bytes == 0)
                {
                    _isConnected = false;
                    throw new ZeroBytesReceivedException("Received zero bytes in a read..");
                }
                for (int i = 0; i < bytes; i++)
                {
                    if (bytesReceived[i] == '\x04')
                    {
                        endIdx = i;
                        break;
                    }
                    else
                    {
                        ms.WriteByte(bytesReceived[i]);
                    }
                }
            }
            sb.Append(Encoding.ASCII.GetString(ms.ToArray()));

            reply = sb.ToString();

            if (isErrorMsg || reply.Contains("M  ERROR"))
            {
                throw new MErrorException(reply);
            }

            return reply;
        }

        public void connect()
        {            
            if (_source == null || String.IsNullOrEmpty(_source.connectionString) || !_source.connectionString.Contains(":"))
            {
                throw new ArgumentException("Invalid source connection string");
            }
            // get host and port from connection string
            String[] hostAndPort = _source.connectionString.Split(new char[] { ':' });
            String host = hostAndPort[0];
            Int32 port = Convert.ToInt32(hostAndPort[1]);

            IPHostEntry hostEntry = Dns.GetHostEntry("localhost");
            IPAddress myIP = (IPAddress)Dns.GetHostEntry(hostEntry.HostName).AddressList[0];

            //Config my client socket and connnect to VistA
            IPAddress vistaIP = null;
            if (!IPAddress.TryParse(host, out vistaIP)) // see if hostname is actually IP address (will get stuck in vistaIP, if so) - if not, get IP address from hostname
            {
                vistaIP = (IPAddress)Dns.GetHostEntry(host).AddressList[0];
            }

            IPEndPoint vistaEndPoint = new IPEndPoint(vistaIP, port);
            _vistaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _vistaSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, VistaRpcConnection.CONNECTION_TIMEOUT);
            try
            {
                _vistaSocket.Connect(vistaEndPoint);
            }
            catch (SocketException)
            {
                throw new VistaRpcConnectionException("There doesn't appear to be a VistA listener at " + _source.connectionString);
            }
            if (!_vistaSocket.Connected)
            {
                throw new VistaRpcConnectionException("Unable to connect to " + _source.connectionString);
            }

            _isConnecting = true;
            //Build the connect request message
            int COUNT_WIDTH = 3;
            string request = "[XWB]10" + COUNT_WIDTH.ToString() + "04\nTCPConnect50" +
                VistaRpcStringUtils.strPack(myIP.ToString(), COUNT_WIDTH) +
                "f0" + VistaRpcStringUtils.strPack(Convert.ToString(0), COUNT_WIDTH) + "f0" +
                VistaRpcStringUtils.strPack(hostEntry.HostName, COUNT_WIDTH) + "f\x0004";

            string reply = "";
            try
            {
                reply = (string)this.query(request);
            }
            catch (SocketException se)
            {
                throw new VistaRpcConnectionException(String.Format("There was a problem when setting up the connection to VistA at {0} - {1}", _source.connectionString, se.Message));
            }
            if (!String.Equals(reply, "accept"))
            {
                _vistaSocket.Close();
                throw new VistaRpcConnectionException("Connection attempt denied by " + _source.connectionString);
            }

            _vistaSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, VistaRpcConnection.READ_TIMEOUT);

            request = "[XWB]11302\x00010\rXUS INTRO MSG54f\x0004";
            reply = (string)this.query(request);

            // set at very end of connect
            _isConnected = true;
            _isConnecting = false;
        }

        public void disconnect()
        {
            try
            {
                string msg = "[XWB]10304\x0005#BYE#\x0004";
                msg = (string)query(msg);
                _vistaSocket.Disconnect(false);
                _vistaSocket.Shutdown(SocketShutdown.Both);
                _vistaSocket.Close();
                _vistaSocket.Dispose();
                base.stopTimer(); // stop the timer so it doesn't get called later!
            }
            catch (Exception) { /* swallow */ }
            finally
            {
                _isConnected = false; // set at very end of every disconnect
                try
                {
                    base.stopTimer();
                }
                catch (Exception) { /* swallow */ }
            }
        }

        public override bool isAlive()
        {
            return _isConnected;
        }

        public override void cleanUp()
        {
            try
            {
                this.disconnect();
            }
            catch (Exception) { }
        }

        public SourceSystem getSource()
        {
            return _source;
        }


        public VistaConnectionStateInfo getStateInfo()
        {
            return VistaConnectionStateInfo.STATEFUL;
        }
    }
}