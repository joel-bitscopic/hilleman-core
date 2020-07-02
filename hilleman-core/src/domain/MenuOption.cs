using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class MenuOption : BaseClass
    {
        public MenuOption() { }

        public String name;
        public bool isPrimary;
        public bool locked;
        public SecurityKey lockKey;
        public String description;
        public User createdBy;
        public String type;

        public IList<MenuOption> children;
        public IList<RemoteProcedure> rpcs;
    }
}