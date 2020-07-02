using System;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista.http
{
    public class VistaHttpRestConnection : IVistaConnection
    {
        SourceSystem _source;

        public VistaHttpRestConnection(SourceSystem source)
        {
            _source = source;
        }

        public SourceSystem getSource()
        {
            return _source;
        }

        public void connect()
        {
            return;
        }

        public void disconnect()
        {
            return;
        }

        public VistaConnectionStateInfo getStateInfo()
        {
            return VistaConnectionStateInfo.NA;
        }
    }
}