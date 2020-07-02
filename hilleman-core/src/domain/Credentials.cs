using System;
using com.bitscopic.hilleman.core.domain.security;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Credentials
    {
        public SourceSystem provider;
        public String username;
        public String password;
        public String email;
        public String tag;
        public Permission permission;

        public Credentials() { }
    }
}