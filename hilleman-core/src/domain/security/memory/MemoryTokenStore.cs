using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.domain.session;
using System.Threading.Tasks;
using com.bitscopic.hilleman.core.dao.vista;

namespace com.bitscopic.hilleman.core.domain.security.memory
{
    /// <summary>
    /// Non-thread-safe memory based token store. All token store implementations should be singletons
    /// </summary>
	public class MemoryTokenStore : ITokenStore, IDisposable
	{
        Dictionary<String, Token> _tokens;
        System.Threading.CancellationTokenSource _cleanupCancelTokenSrc;

        #region Singleton
        public static MemoryTokenStore getInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new MemoryTokenStore();
                    }
                }
            }
            return _instance;
        }

        private static readonly object _locker = new object();
        private static MemoryTokenStore _instance;
        private MemoryTokenStore() 
        {
            _tokens = new Dictionary<string, Token>();
            loadAppTokens();
            if (StringUtils.parseBool(MyConfigurationManager.getValue("UseTokenCleanupThread")) && !StringUtils.parseBool(MyConfigurationManager.getValue("UseTokenTimers")))
            {
                startTokenCleanupThread();
            }
        }
        #endregion

        void loadAppTokens()
        {
            _tokens.Add("FCvXWAo/gyVAm1w+BlAIFrkBer+ElKTGB40S94ad8Wg=", new security.Token() { immutableExpiration = new DateTime(2017, 12, 31), issued = new DateTime(2017, 6, 26) });
        }

        private void startTokenCleanupThreadLoop(ITokenStore store)
        {
            while (!_cleanupCancelTokenSrc.IsCancellationRequested)
            {
                try
                {
                    TokenStoreUtils.cleanUpTokens(store);
                }
                catch (Exception) { }
                System.Threading.Thread.Sleep(60 * 1000);
            }
        }

        private void startTokenCleanupThread()
        {
            _cleanupCancelTokenSrc = new System.Threading.CancellationTokenSource();
            Task t = new Task(() => startTokenCleanupThreadLoop(this), _cleanupCancelTokenSrc.Token);
            t.ContinueWith(rslt => exceptionSink(t), TaskContinuationOptions.OnlyOnFaulted);
            t.Start();
        }

        private Task exceptionSink(Task t)
        {
            if (t.IsFaulted || t.Exception != null)
            {
                // do nothing! just observe which makes faulted task happy
            }
            return t;
        }

        public Token getToken(string tokenId)
        {
            // TODO - for debugging - remove this eventually
            if (String.Equals("TRUE", MyConfigurationManager.getValue("FreeForAll"), StringComparison.CurrentCultureIgnoreCase))
            {
                Token t = createNewToken(new HillemanSession() { endUser = new User() { id = "1" } });
                SourceSystemTable table = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));
                HillemanSession tokenState = ((HillemanSession)t.state); // grab a reference so we don't have to cast for every site in the SST
                foreach (SourceSystem ss in table.sources)
                {
                    tokenState.addAuthorizedConnection(new VistaConnectionFactory().getVistaConnection(ss));
                }
                return t;
            }

            // TODO - check token is still valid/revoke if needed
            if (_tokens.ContainsKey(tokenId))
            {
                Token t = _tokens[tokenId];
                t.access();
                t.resetTimer(); // resets
                return t;
            }
            return null;
        }

        public void revokeToken(string tokenId)
        {
            if (_tokens.ContainsKey(tokenId))
            {
                Token trash = _tokens[tokenId];
                _tokens.Remove(tokenId);
                trash.Dispose();
            }
        }

        public Token createNewToken(object state)
        {
            TimeSpan tokenTimeout = new TimeSpan(0, 30, 0); // TODO - make this configurable!
            TimeSpan.TryParse(MyConfigurationManager.getValue("TokenTimeout"), out tokenTimeout); // TODO - don't parse this every time. maybe create a class variable?

            Token t = new Token()
            {
                timeout = tokenTimeout, 
                issued = DateTime.Now,
                lastAccessed = DateTime.Now,
                maxAccesses = Int32.MaxValue, // TODO - make this configurable
                state = state,
                value = utils.CryptographyUtils.createRandomHashBase64()
            };

            // allow a hard expiration to be set on tokens - e.g. all tokens expire in 6 hours
            if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("TokenImmutableTimeout")))
            {
                t.immutableExpiration = DateTime.Now.Add(TimeSpan.Parse(MyConfigurationManager.getValue("TokenImmutableTimeout")));
            }

            if (StringUtils.parseBool(MyConfigurationManager.getValue("UseTokenTimers")))
            {
                t.startTokenTimer(this);
            }
            t.tokenHash = CryptographyUtils.hmac256Hash(t.value, t);
            _tokens.Add(t.value, t); // use value and not token hash - hash is just for integrity
            return t;
        }

        public Token updateToken(String tokenId, object newState, bool updateLastAccessed = true)
        {
            if (!_tokens.ContainsKey(tokenId))
            {
                throw new ArgumentException("Invalid token ID");
            }

            Token t = getToken(tokenId);
            if (updateLastAccessed)
            {
                t.lastAccessed = DateTime.Now;
            }

            if (newState != null)
            {
                t.state = newState;
            }

            return t;
        }


        public IList<Token> getAll()
        {
            return new List<Token>(_tokens.Values);
        }


        public void Dispose()
        {
            _cleanupCancelTokenSrc.Cancel();
        }

        public void addRequest(string tokenId, string request, string serializedRequest, String serializedResponse)
        {
            // TBD - do anything here? kinda 1/2 implemented right now... 1/9/2018
        }
    }
}