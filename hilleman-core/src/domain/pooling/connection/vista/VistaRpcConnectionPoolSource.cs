using System;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPoolSource : AbstractPoolSource
    {
        public SourceSystem CxnSource { get; set; }
        public Credentials Credentials { get; set; }
        public VistaRpcConnectionBrokerContext BrokerContext { get; set; }
    }
}
