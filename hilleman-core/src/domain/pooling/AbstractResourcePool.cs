using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.bitscopic.hilleman.core.domain.pooling
{
    public abstract class AbstractResourcePool
    {
        private int _totalResources;
        public int TotalResources 
        {
            get { return _totalResources; } 
        }

        public void decrementResourceCount()
        {
            System.Threading.Interlocked.Decrement(ref _totalResources);
        }

        public void incrementResourceCount()
        {
            System.Threading.Interlocked.Increment(ref _totalResources);
        }

        public AbstractPoolSource PoolSource { get; set; }

        public abstract AbstractResource checkOut(object obj);

        public abstract AbstractResource checkOutAlive(object obj);

        public abstract object checkIn(AbstractResource objToReturn);

        public abstract void shutdown();
    }
}
