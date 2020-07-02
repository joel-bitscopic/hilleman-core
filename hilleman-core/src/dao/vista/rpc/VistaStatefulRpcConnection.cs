using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaStatefulRpcConnection : VistaRpcConnection
    {
        public VistaStatefulRpcConnection(SourceSystem source) : base(source)
        {

        }

        public new VistaConnectionStateInfo getStateInfo()
        {
            return VistaConnectionStateInfo.STATEFUL;
        }

    }
}