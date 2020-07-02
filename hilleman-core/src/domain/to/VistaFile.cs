using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.to
{
    [Serializable]
    public class VistaFile
    {
        public String number;
        public String name;
        public String global;

        public IList<VistaFieldDef> fieldDefs;
    }
}