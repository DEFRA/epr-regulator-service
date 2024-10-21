using System.ComponentModel;

namespace EPR.RegulatorService.Frontend.Core.Enums;

public enum RegistrationSubmissionOrganisationType
{
    [Description("Not specified")]
    none = 0,
    [Description("Compliance scheme")]
    compliance = 1,
    [Description("Large producer")]
    large = 10,
    [Description("Small producer")]
    small = 20
}