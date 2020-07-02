using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao.vista
{
    public class CachedRequest
    {
        ICrrudDao _dao;

        public CachedRequest(ICrrudDao dao, String requestString)
        {
            _dao = dao;
        }

        
    }
}