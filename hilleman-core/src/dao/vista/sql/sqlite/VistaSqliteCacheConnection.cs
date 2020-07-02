using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista.sql.sqlite
{
    public class VistaSqliteCacheConnection : IVistaConnection
    {
        SourceSystem _src;

        public VistaSqliteCacheConnection(SourceSystem source)
        {
            _src = source;
        }

        public void connect()
        {
            //throw new NotImplementedException();
        }

        public void disconnect()
        {
            //throw new NotImplementedException();
        }

        public domain.SourceSystem getSource()
        {
            return _src;
        }


        public VistaConnectionStateInfo getStateInfo()
        {
            return VistaConnectionStateInfo.NA;
        }
    }
}