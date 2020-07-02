using System;

namespace com.bitscopic.hilleman.core.domain.exception
{
    public class HillemanBaseException : Exception, System.Runtime.Serialization.ISerializable
    {
        public String errorCode;
        public Exception inner;
        public object extraInfo;

        public HillemanBaseException() : base()
        {
        }

        /// <summary>
        /// This constructor is required to deserialize this custom exception properly. Any new properties should be added
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public HillemanBaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base()
        {
            errorCode = info.GetString("errorCode");
            extraInfo = info.GetValue("extraInfo", typeof(object));
        }

        public HillemanBaseException(String message) : base(message)
        {
        }

        public HillemanBaseException(String message, String errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// This method is required to serialize the custom properties of the exception. New properties should be added here to ensure they are included in serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("errorCode", this.errorCode);
            info.AddValue("extraInfo", this.extraInfo);
            base.GetObjectData(info, context);
        }

    }
}