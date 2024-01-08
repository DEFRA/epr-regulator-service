using System.ComponentModel;

namespace EPR.RegulatorService.Frontend.Core.Enums;

public enum OrganisationType
{
    [Description("Direct Producer")]
    DirectProducer = 1,
    [Description("Compliance Scheme")]
    ComplianceScheme = 2,
}
