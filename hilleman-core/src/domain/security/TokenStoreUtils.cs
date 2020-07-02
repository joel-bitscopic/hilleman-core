using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain.session;
using com.bitscopic.hilleman.core.dao.logging;

namespace com.bitscopic.hilleman.core.domain.security
{
    public static class TokenStoreUtils
    {
        /// <summary>
        /// Look for expired and timedout tokens. Remove them from the store and save them to the logging DB
        /// </summary>
        /// <param name="store"></param>
        public static void cleanUpTokens(ITokenStore store)
        {
            TimeSpan defaultTokenTimeout = new TimeSpan(0, 30, 0);
            TimeSpan.TryParse(MyConfigurationManager.getValue("TokenTimeout"), out defaultTokenTimeout);

            IList<Token> allTokens = store.getAll();
            IList<Token> tokensToRemove = new List<Token>();
            foreach (Token t in allTokens)
            {
                if (t.immutableExpiration.Year > 2000 && DateTime.Now > t.immutableExpiration)
                {
                    tokensToRemove.Add(t);
                    // token expired
                }
                else if (DateTime.Now.Subtract(t.lastAccessed) > defaultTokenTimeout)
                {
                    tokensToRemove.Add(t);
                    // token timed out
                }
            }

            foreach (Token t in tokensToRemove)
            {
                store.revokeToken(t.value);
                HillemanSession tokenSession = (HillemanSession)t.state;
                tokenSession.sessionEnd = DateTime.Now;
                SessionLoggingFactory.getSessionDao().saveSessionAsync(tokenSession);
            }
        }
    }
}