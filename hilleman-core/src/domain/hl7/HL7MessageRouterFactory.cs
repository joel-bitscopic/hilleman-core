using System;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    public static class HL7MessageRouterFactory
    {
        private static IHL7MessageRouter _singletonRouter;
        private static readonly object _locker = new byte();

        /// <summary>
        /// Singleton HL7 message router helper - a process contains a single instance of a IHL7MessageRouter
        /// </summary>
        /// <returns></returns>
        public static IHL7MessageRouter getMessageRouter()
        {
            if (_singletonRouter == null)
            {
                _singletonRouter = new com.bitscopic.hilleman.core.domain.hl7.TestHL7MessageRouter();
                String messageRouterType = MyConfigurationManager.getValue("HL7_MESSAGE_ROUTER_TYPE");

                if (String.IsNullOrEmpty(messageRouterType) || String.Equals("lib", messageRouterType, StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_locker)
                    {
                        if (_singletonRouter == null)
                        {
                            //_singletonRouter = implement your HL7 message router
                        }
                    }
                }
                else if (String.Equals("service", messageRouterType, StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_locker)
                    {
                        if (_singletonRouter == null)
                        {
                            //_singletonRouter = implement your HL7 message router
                        }
                    }
                }
                else if (String.Equals("test", messageRouterType, StringComparison.CurrentCultureIgnoreCase))
                {
                    lock (_locker)
                    {
                        if (_singletonRouter == null)
                        {
                            _singletonRouter = new com.bitscopic.hilleman.core.domain.hl7.TestHL7MessageRouter();
                        }
                    }
                }
            }

            return _singletonRouter;
        }
    }
}