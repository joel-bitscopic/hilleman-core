using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.vista
{
    [Serializable]
    public class FmFile
    {
        public string number;
        public string name;
        public string global;
        public List<FmField> fields;

        public FmFile() { }
    }
}