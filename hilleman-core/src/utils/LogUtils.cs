using com.bitscopic.hilleman.core.domain;
using System;
using System.IO;

namespace com.bitscopic.hilleman.core.utils
{
    public static class LogUtils
    {
        static readonly String logPath = String.IsNullOrEmpty(MyConfigurationManager.getValue("LogUtilsPath")) ? "hilleman.log" : MyConfigurationManager.getValue("LogUtilsPath");

        public static void LOG(String message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    sw.WriteLine(message);
                }
            }
            catch (Exception) { /* swallow! */ }
        }

        static object _locker = new object();
        static System.Collections.Concurrent.ConcurrentQueue<String> _debugMsgs = new System.Collections.Concurrent.ConcurrentQueue<string>();
        static readonly Int32 _msgBufferSize = 1;
        public static void debug(String message)
        {
            _debugMsgs.Enqueue(message);

            if (_debugMsgs.Count >= _msgBufferSize)
            {
                lock(_locker)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(logPath, true))
                        {
                            String currentMsg = null;
                            while (_debugMsgs.TryDequeue(out currentMsg))
                            {
                                sw.WriteLine(currentMsg);
                            }
                        }
                    }
                    catch (Exception) { /* swallow! */ }

                    _debugMsgs = new System.Collections.Concurrent.ConcurrentQueue<String>();
                }
            }
        }
    }
}