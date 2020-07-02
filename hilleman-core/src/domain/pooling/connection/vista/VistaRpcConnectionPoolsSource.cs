using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    public class VistaRpcConnectionPoolsSource : AbstractPoolSource
    {
        public Dictionary<string, VistaRpcConnectionPoolSource> CxnSources { get; set; }
    }
}
