using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.security
{
    public interface ITokenStore
    {
        /// <summary>
        /// Fetch a token by token ID
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns>Token if token ID exists in token store. NULL otherwise</returns>
        Token getToken(String tokenId);

        /// <summary>
        /// Revoke a token. Token store implementation determines the internal implementation but
        /// should always place token in a state where subsequent calls for the token ID return NULL
        /// </summary>
        /// <param name="tokenId"></param>
        void revokeToken(String tokenId);

        /// <summary>
        /// Create a new token
        /// </summary>
        /// <param name="state">The state of the object, if any</param>
        /// <returns></returns>
        Token createNewToken(object state);

        /// <summary>
        /// Update a token. All token stores should only allow last accessed and state to be updated
        /// </summary>
        /// <param name="lastAccessed"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        Token updateToken(String tokenId, object newState, bool updateLastAccessed = true);

        /// <summary>
        /// Get a list of all the tokens in the store
        /// </summary>
        /// <returns></returns>
        IList<Token> getAll();

        void addRequest(String tokenId, String request, String requestContents, String serializedResponse);
    }
}
