using System;

namespace com.bitscopic.hilleman.core.domain.to
{
    [Serializable]
    public class ReadRange : BaseClass
    {
        public String apiName;
        public String file;
        public String fields;
        public String iens;
        public String flags;
        public String xref;
        public String maxRex;
        public String from;
        public String screen;
        public String identifier;
    }
}