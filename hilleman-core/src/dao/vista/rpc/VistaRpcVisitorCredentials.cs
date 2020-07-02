using System;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    [Serializable]
    public class VistaRpcVisitorCredentials : Credentials
    {
        public VistaRpcVisitorCredentials() { }

        public User visitor;
    }
}