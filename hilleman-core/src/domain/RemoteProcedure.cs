using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class RemoteProcedure : BaseClass
    {
        public RemoteProcedure() { }

        public String name;
        public String tag;
        public String routine;
        public String returnValueType;
        public String version;
        public String description;
        public List<RemoteProcedureParameter> inputParameters;
        public String responseDescription;
    }
}