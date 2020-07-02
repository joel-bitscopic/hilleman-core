using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.exception;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.to;
using com.bitscopic.hilleman.core.filters;
using com.bitscopic.hilleman.core.utils;
using Microsoft.AspNetCore.Mvc;

namespace com.bitscopic.hilleman.core.svc
{
    [Route("svc/hilleman.svc")]
    [ApiController]
    //[ProtectedAPI] // not strictly needed on both class and method definitions but it doesn't hurt
    public class HillemanController : ControllerBase
    {
        [ProtectedAPI]
        [HttpGet("configs")]
        public String getConfigs()
        {
            return SerializerUtils.serialize(MyConfigurationManager.configsByKey, false);
        }

        [ProtectedAPI]
        [HttpPost("configs")]
        public String updateConfig([FromBody] object request)
        {
            try
            {
                List<NameValue> configsToSet = SerializerUtils.deserialize<List<NameValue>>(SerializerUtils.serialize(request));
                foreach (NameValue nv in configsToSet)
                {
                    MyConfigurationManager.setValue(nv.name, nv.value);
                }

                return SerializerUtils.serialize(MyConfigurationManager.configsByKey, false);
            }
            catch (Exception exc)
            {
                return SerializerUtils.serialize(new HillemanBaseException(exc.Message));
            }
        }

        [ProtectedAPI]
        [HttpPost("reloadConfigs")]
        public String reloadConfigs()
        {
            try
            {
                MyConfigurationManager.reloadConfigs();
                return SerializerUtils.serialize(MyConfigurationManager.configsByKey, false);
            }
            catch (Exception exc)
            {
                return SerializerUtils.serialize(new HillemanBaseException(exc.Message));
            }
        }

        [ProtectedAPI]
        [HttpGet("activeUserSessions")]
        public String getActiveUserSessions()
        {
            try
            {
                IList<Token> allTokens = TokenStoreFactory.getTokenStore(HttpContext.Request.Path.Value).getAll();
                List<Token> result = new List<Token>();
                foreach (Token t in allTokens)
                {
                    //if (t.state is PraediGeneUser)
                    //{
                    //    result.Add(new Token() { immutableExpiration = t.immutableExpiration, issued = t.issued, lastAccessed = t.lastAccessed, timeout = t.timeout, value = t.value, state = t.state });
                    //}
                }

                return SerializerUtils.serialize(result, false);
            }
            catch (Exception e)
            {
                return SerializerUtils.serialize(new RequestFault("Error fetching user sessions", e), false);
            }
        }

        [SecuredAPI(isSecured: false)]
        [HttpGet("assemblyVersion")]
        public String getAssemblyVersion()
        {
            return SerializerUtils.serialize(this.GetType().Assembly.ToString());
        }

        [SecuredAPI(isSecured: false)]
        [HttpGet("runtime")]
        public String getRuntime()
        {
            return SerializerUtils.serialize(this.GetType().Assembly.ImageRuntimeVersion);
        }
    }
}