using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.exception
{
    [Serializable]
    public class ServiceResponseException
    {
        public bool isException;
        public Exception innerException;
        public String message;
        public String help;

        public ServiceResponseException() 
        {
            this.isException = true;
        }

        public bool isServiceResponseException(object obj)
        {
            return obj is ServiceResponseException;
        }

        public bool isServiceResponseException(System.IO.Stream stream)
        {
            try
            {
                if (com.bitscopic.hilleman.core.utils.SerializerUtils.deserializeFromStream<ServiceResponseException>(stream) != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception) { }
            {
                return false;
            }
        }
    }
}