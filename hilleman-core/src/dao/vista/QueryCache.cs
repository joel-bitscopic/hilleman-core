using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao
{
    public class QueryCache
    {
        Dictionary<String, ReadResponse> _readResponses = new Dictionary<string, ReadResponse>();
        Dictionary<String, ReadRangeResponse> _readRangeResponses = new Dictionary<string, ReadRangeResponse>();

        #region Singleton
        public static QueryCache getInstance()
        {
            if (_singleton == null)
            {
                lock (_locker)
                {
                    if (_singleton == null)
                    {
                        _singleton = new QueryCache();
                    }
                }
            }
            return _singleton;
        }

        private static readonly object _locker = new object();
        private static QueryCache _singleton;

        private QueryCache()
        {
        }
        #endregion

        public ReadResponse cachedRead(ReadRequest request, ICrrudDao dao)
        {
            String siteId = dao.getSource().id;
            String requestHash = CryptographyUtils.hmac256Hash(siteId, request);

            if (_readResponses.ContainsKey(requestHash))
            {
                return _readResponses[requestHash];
            }
            else
            {
                ReadResponse response = dao.read(request);
                _readResponses.Add(requestHash, response);
                return response;
            }
        }
    }
}