using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class RemoteProcedureParameter : BaseClass
    {
        public RemoteProcedureParameter() { }

        public String name;
        public String type;
        public Int32 maximumDataLength;
        public bool required;
        public String description;
    }
}