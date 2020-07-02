using System;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaUserRpcConnectionPoolSource : VistaRpcConnectionPoolSource
    {
        public User EndUser { get; set; }

        public VistaUserRpcConnectionPoolSource() : base() { }
    }
}
