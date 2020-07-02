using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class SecurityKey : BaseClass
    {
        public SecurityKey() { }

        public String name;
        public String descriptiveName;

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SecurityKey))
            {
                return false;
            }
            SecurityKey casted = (SecurityKey)obj;
            return (String.Equals(casted.id, this.id) && String.Equals(casted.name, this.name) && String.Equals(casted.sourceSystemId, this.sourceSystemId));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}