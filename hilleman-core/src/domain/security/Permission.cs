using System;

namespace com.bitscopic.hilleman.core.domain.security
{
    [Serializable]
    public class Permission
    {
        public Permission(String permissionId, String permissionName)
        {
            this.id = permissionId;
            this.name = permissionName;
        }

        public String id;
        public String name;
    }
}