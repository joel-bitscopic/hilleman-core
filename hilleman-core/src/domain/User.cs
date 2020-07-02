using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain.security;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class User : Person
    {
        public List<SecurityKey> keys;
        public List<MenuOption> options;

        [NonSerialized]
        public Token token;

        public User() { }

    }
}