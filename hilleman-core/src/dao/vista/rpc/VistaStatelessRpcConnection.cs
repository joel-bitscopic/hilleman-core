using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaStatelessRpcConnection : VistaRpcConnection, IVistaConnection
    {
        public VistaStatelessRpcConnection(SourceSystem source)
            : base(source)
        {
        }

        public new VistaConnectionStateInfo getStateInfo()
        {
            return VistaConnectionStateInfo.STATELESS;
        }
    }
}