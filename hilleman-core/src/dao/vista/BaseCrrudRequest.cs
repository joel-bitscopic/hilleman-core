using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.dao
{
    public class BaseCrrudRequest
    {
        SourceSystem _source;

        public BaseCrrudRequest(SourceSystem source)
        {
            _source = source;
        }

        public SourceSystem getSource()
        {
            return _source;
        }

        public object execute(IVistaConnection cxn)
        {
            throw new NotImplementedException();
//            if (cxn.
        }
    }
}