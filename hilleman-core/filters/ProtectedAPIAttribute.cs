using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.to;
using com.bitscopic.hilleman.core.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace com.bitscopic.hilleman.core.filters
{
    /// <summary>
    /// Attribute class for explicitly protecting APIs - this attribute will prevent a call being made if a protected token is not present with the request
    /// </summary>
    public class ProtectedAPIAttribute : ActionFilterAttribute // ResultFilterAttribute
    {
        public ProtectedAPIAttribute() { }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (String.Equals("TRUE", MyConfigurationManager.getValue("FreeForAll"), StringComparison.CurrentCultureIgnoreCase)) // only skip validation if we've explicitly set this to true
            {
                base.OnActionExecuting(context);
                return;
            }

            try
            {
                String requestPassword = context.HttpContext.Request.Headers["Access-Control-App-Token"];
                if (String.IsNullOrEmpty(requestPassword))
                {
                    setContentResult(context, "", 400);
                    return;
                }

                ITokenStore tokenStore = TokenStoreFactory.getTokenStore(context.HttpContext.Request.Path.Value);
                Token requestToken = tokenStore.getToken(requestPassword);

                if (requestToken == null || !(requestToken.state is ApiKey)) // note token should correspond to ApiKey which are loaded at PG startup
                {
                    String serializedResponse = SerializerUtils.serialize(new RequestFault(message: "Invalid token", errorCode: "401", innerExc: new UnauthorizedAccessException()));
                    setContentResult(context, serializedResponse, 401);
                    return;
                }
            }
            catch (Exception exc)
            {
                String serializedResponse = SerializerUtils.serialize(new RequestFault(message: "Internal error: " + exc.ToString() + "\r\nStack Trace: " + exc.StackTrace, errorCode: "500", innerExc: exc));
                setContentResult(context, serializedResponse, 500);
                return;
            }

            base.OnActionExecuting(context);
        }

        protected void setContentResult(ActionExecutingContext context, String content, Int32 statusCode)
        {
            context.Result = new ContentResult() { Content = content, ContentType = "application/json", StatusCode = statusCode };
        }

    }
}
