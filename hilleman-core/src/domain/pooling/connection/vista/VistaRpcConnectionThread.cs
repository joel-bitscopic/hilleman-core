using System;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionThread : AbstractResourceThread
    {
        public VistaRpcConnectionThread() 
        { 
            this.Timestamp = DateTime.Now; 
        }

        public IVistaConnection Connection;
    }
}
