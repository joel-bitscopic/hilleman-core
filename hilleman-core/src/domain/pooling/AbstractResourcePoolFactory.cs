using System;

namespace com.bitscopic.hilleman.core.domain.pooling
{
    public abstract class AbstractResourcePoolFactory
    {
        public abstract AbstractResourcePool getResourcePool(AbstractPoolSource source);
    }
}
