using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPoolSourceFactory : AbstractPoolSourceFactory
    {
        /// <summary>
        /// Use to set hard coded pool defaults
        /// </summary>
        public VistaRpcConnectionPoolSourceFactory() : base()
        {
            this.Default = new VistaRpcConnectionPoolSource()
            {
                LoadStrategy = LoadingStrategy.Lazy,
                MaxPoolSize = 8,
                MinPoolSize = 2,
                PoolExpansionSize = 2,
                WaitTime = new TimeSpan(0, 1, 0)
            };
        }

        public VistaRpcConnectionPoolSourceFactory(AbstractPoolSource defaultSource) : base(defaultSource) { }

        public override object getPoolSources(object sources)
        {
            throw new NotImplementedException();
        }

        public override AbstractPoolSource getPoolSource(object source)
        {
            if (!(source is SourceSystem))
            {
                throw new ArgumentException("Invalid source");
            }

            VistaRpcConnectionPoolSource theSrc = new VistaRpcConnectionPoolSource();
            theSrc.CxnSource = (SourceSystem)source;
            theSrc.LoadStrategy = this.Default.LoadStrategy;
            theSrc.MaxPoolSize = this.Default.MaxPoolSize;
            theSrc.MinPoolSize = this.Default.MinPoolSize;
            theSrc.PoolExpansionSize = this.Default.PoolExpansionSize;
            theSrc.WaitTime = this.Default.WaitTime;
            return theSrc;
        }

    }
}
