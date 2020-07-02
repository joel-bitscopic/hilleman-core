using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class NameValue
    {
        public String name;
        public String value;

        public NameValue() { }

        public NameValue(String name, String value)
        {
            this.name = name;
            this.value = value;
        }
    }
}