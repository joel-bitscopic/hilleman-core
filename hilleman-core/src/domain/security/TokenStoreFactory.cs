using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.security
{
    public class TokenStoreFactory
    {
        // TODO - if new token stores are implemented
        public static ITokenStore getTokenStore()
        {
            return com.bitscopic.hilleman.core.domain.security.memory.MemoryTokenStore.getInstance();
        }

        public static ITokenStore getTokenStore(String requestUrl)
        {
            if (false) // if you want a different token store implementation, fill in details here
            {
                // return com.bitscopic.hilleman.core.domain.security.myTokenStor.MyTokenStoreName.getInstance();
            }
            else
            {
                return com.bitscopic.hilleman.core.domain.security.memory.MemoryTokenStore.getInstance();
            }
        }
    }
}