using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public enum PermissionType
    {
        NotSet = 0,
        Basic = 1,
        Admin = 2,
        Delegated = 3,
        Approved = 4
    }
}
