using com.bitscopic.hilleman.core.domain.security;
using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.dao.iface.praedigene
{
    public interface IAppSecurityDao
    {
        ApiKey getApiKey(String key);

        IList<ApiKey> getAllApiKeys();

        IList<Token> getActiveAppSessions();

        void updateSession(String tokenId, DateTime lastAccessed);

        void saveNewToken(Token t);
    }
}
