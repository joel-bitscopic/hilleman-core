using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaPooledRpcConnectionFactory
    {
        #region Singleton
        private VistaPooledRpcConnectionFactory() { }
        private static object _locker = new object();
        private static VistaPooledRpcConnectionFactory _instance = null;

        public static VistaPooledRpcConnectionFactory getInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new VistaPooledRpcConnectionFactory();
                    }
                }
            }

            return _instance;
        }
        #endregion

        public IVistaConnection getConnection(AbstractPoolSource poolSource)
        {
            if (poolSource is VistaUserRpcConnectionPoolSource)
            {
                VistaUserRpcConnection cxn = new VistaUserRpcConnection(((VistaUserRpcConnectionPoolSource)poolSource).CxnSource);
                cxn.user = ((VistaUserRpcConnectionPoolSource)poolSource).EndUser;
                return cxn;
            }
            else if (poolSource is VistaRpcConnectionPoolSource)
            {
                VistaRpcConnection cxn = new VistaRpcConnection(((VistaRpcConnectionPoolSource)poolSource).CxnSource);
                return cxn;
            }
            else
            {
                throw new NotImplementedException("Pooled RPC connection factory not currently implemented for " + poolSource.GetType().FullName);
            }
        }
    }
}
