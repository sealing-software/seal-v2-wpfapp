using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEAL_V2.model
{
    class Permission
    {
        private String permissionName;
        private bool permitted = false;

        public Permission(String permissionName, bool permitted)
        {
            this.permissionName = permissionName;
            this.permitted = permitted;
        }

        public String getPermissionName()
        {
            return permissionName;
        }

    }
}
