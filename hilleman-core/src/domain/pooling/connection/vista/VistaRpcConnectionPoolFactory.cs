using System;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPoolFactory : AbstractResourcePoolFactory
    {

        public override AbstractResourcePool getResourcePool(AbstractPoolSource source)
        {
            if (source == null)
            {
                throw new ArgumentException("Need to supply pool source before connection pool can be built");
            }
            VistaRpcConnectionPools pool = VistaRpcConnectionPools.getInstance(source);
            return pool;
        }
    }
}
