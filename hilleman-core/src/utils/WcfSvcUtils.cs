using com.bitscopic.hilleman.core.domain.exception;
using com.bitscopic.hilleman.core.domain.security;
using com.bitscopic.hilleman.core.domain.to;
using System;

namespace com.bitscopic.hilleman.core.utils
{
    public static class WcfSvcUtils
    {

        public static String makeCallForWebAPI(Delegate d, object[] args, bool includeNullsInSerializedResult = true)
        {
            try
            {
                return SerializerUtils.serialize(d.DynamicInvoke(args), includeNullsInSerializedResult);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                if (tie.GetBaseException() is HillemanBaseException)
                {
                    return SerializerUtils.serialize(new RequestFault((HillemanBaseException)tie.GetBaseException()), includeNullsInSerializedResult);
                }
                else
                {
                    return SerializerUtils.serialize(new RequestFault(tie.GetBaseException().Message), includeNullsInSerializedResult);
                }
            }
            catch (ArgumentException) // wrong args supplied to delegate - mistake in code!!
            {
                // TBD - log?
                return SerializerUtils.serialize(new RequestFault("There appears to be a problem with this call. Please contact tech support"), false);
            }
            catch (System.Reflection.TargetParameterCountException) // wrong number of args supplied to delegate - mistake in code!!
            {
                // TBD - log?
                return SerializerUtils.serialize(new RequestFault("There appears to be a problem with this call. Please contact tech support"), false);
            }
            catch (HillemanBaseException hbe)
            {
                return SerializerUtils.serialize(new RequestFault(hbe), false);
            }
            catch (Exception e)
            {
                return SerializerUtils.serialize(new RequestFault(message: e.Message, innerExc: e), includeNullsInSerializedResult);
            }
        }

    }
}