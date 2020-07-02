using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.to;
using com.bitscopic.hilleman.core.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace com.bitscopic.hilleman.core.filters
{
    /// <summary>
    /// Attribute class for explcitly securing APIs
    /// </summary>
    public class SecuredAPIAttribute : ActionFilterAttribute 
    {
        private readonly bool _isSecured;

        public SecuredAPIAttribute(bool isSecured)
        {
            _isSecured = isSecured;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (String.Equals("TRUE", MyConfigurationManager.getValue("FreeForAll"), StringComparison.CurrentCultureIgnoreCase)) // only skip validation if we've explicitly set this to true
            {
                base.OnActionExecuting(context);
                return;
            }
            else if (_isSecured)
            {
                // the default .NET Core behavior is to apply class attributes first as the request is processed. so, if you set an attribute at the class (controller) definition level
                // and then set a "conflicting" attribute on a method (API), the class filter may end up short-circuiting the method attribute specificiation. more concretely, if this 
                // SecuredAPIAttribute is set at a class level to avoid writing the attribute on every API signature but then we would like to set the isSecured property to 'false'
                // on some APIs in that controller, the API level attribute would never be checked since the entire controller/class was secured. i think the MS/.NET behavior here is generally
                // correct and logically makes sense but i also don't think it's desirable in this case. sooo... this little loop checks to see if there are multiple SecuredAPIAttrbute filters and
                // if any of them are set to not secured then allow the request to proceed
                foreach (var filter in context.Filters)
                {
                    if (filter is SecuredAPIAttribute)
                    {
                        if (((SecuredAPIAttribute)filter)._isSecured == false)
                        {
                            base.OnActionExecuting(context);
                            return;
                        }
                    }

                    if (filter is ProtectedAPIAttribute) // ProtectedAPIAttribute is "higher" auth level than secured - don't bother with code below is attribute is set - ProtectedAPIAttribute filter should handle rules
                    {
                        base.OnActionExecuting(context);
                        return;
                    }
                }

                HttpContext httpContext = context.HttpContext;
                try
                {
                    String token = httpContext.Request.Headers[com.bitscopic.hilleman.core.domain.HillemanConstants.TokenName];

                    if (String.IsNullOrEmpty(token))
                    {
                        String serializedResponse = SerializerUtils.serialize(new RequestFault(message: "No token found for request", errorCode: "400", innerExc: new UnauthorizedAccessException()));
                        setContentResult(context, serializedResponse, 400);
                        return;
                    }

                    ITokenStore tokenStore = TokenStoreFactory.getTokenStore(httpContext.Request.Path.Value);
                    Token requestToken = tokenStore.getToken(token);

                    // finally can verify token is valid - use the URL to determine which token store to use
                    if (requestToken == null)
                    {
                        String serializedResponse = SerializerUtils.serialize(new RequestFault(message: "Invalid token", errorCode: "401", innerExc: new UnauthorizedAccessException()));
                        setContentResult(context, serializedResponse, 401);
                        return;
                    }

                    // token is good!! handle any session bits and allow request to continue
                    tokenStore.addRequest(token, httpContext.Request.Path.Value, null, null);
                }
                catch (Exception exc)
                {
                    String serializedResponse = SerializerUtils.serialize(new RequestFault(message: "Internal error: " + exc.ToString() + "\r\nStack Trace: " + exc.StackTrace, errorCode: "500", innerExc: exc));
                    setContentResult(context, serializedResponse, 500);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        protected void setContentResult(ActionExecutingContext context, String content, Int32 statusCode)
        {
            context.Result = new ContentResult() { Content = content, ContentType = "application/json", StatusCode = statusCode };
        }
    }
}
