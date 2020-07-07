using com.bitscopic.hilleman.core.lib;
using com.bitscopic.hilleman.core.utils;
using Microsoft.AspNetCore.Mvc;
using System;

namespace com.bitscopic.hilleman.core.svc
{
    [Route("svc/rpa.svc")]
    [ApiController]
    public class RPAController
    {
        [HttpPost("invokeRPA")]
        public String invokeRPA([FromBody] object requestBody)
        {
            return WcfSvcUtils.makeCallForWebAPI(
                new Func<String, String>(new RPALib().invokeRPA), new object[] { SerializerUtils.serialize(requestBody) }, false);
        }
    }
}
