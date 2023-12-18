using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.RegulatorService.Frontend.Core.Sessions
{
    public enum RelationshipWithOrganisation
    {
        NotSet = 0,
        Employee = 1,
        Consultant = 2,
        ConsultantFromComplianceScheme = 3,
        SomethingElse = 4
    }
}
