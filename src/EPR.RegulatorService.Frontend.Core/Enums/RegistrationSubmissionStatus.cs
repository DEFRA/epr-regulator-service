using System.ComponentModel;

namespace EPR.RegulatorService.Frontend.Core.Enums;

public enum RegistrationSubmissionStatus
{
    [Description("RegistrationSubmissionStatus.NotSpecified")]
    None = 0,

    [Description("RegistrationSubmissionStatus.Pending")]
    Pending = 1,

    [Description("RegistrationSubmissionStatus.Granted")]
    Granted = 10,

    [Description("RegistrationSubmissionStatus.Refused")]
    Refused = 20,

    [Description("RegistrationSubmissionStatus.Queried")]
    Queried = 30,

    [Description("RegistrationSubmissionStatus.Cancelled")]
    Cancelled = 40,

    [Description("RegistrationSubmissionStatus.Updated")]
    Updated = 50
}