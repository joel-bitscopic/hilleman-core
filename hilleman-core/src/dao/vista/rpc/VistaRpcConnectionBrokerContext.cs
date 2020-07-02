using System;
using com.bitscopic.hilleman.core.domain.security;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    [Serializable]
    public class VistaRpcConnectionBrokerContext : Permission
    {
        public VistaRpcConnectionBrokerContext(String permissionId, String permissionName) : base(permissionId, permissionName) { }
    }
}