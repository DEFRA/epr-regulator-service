using System.ComponentModel;

namespace EPR.RegulatorService.Frontend.Core.Enums;

public enum RegistrationSubmissionStatus
{
    [Description("RegistrationSubmissionStatus.NotSpecified")]
    none = 0,
    [Description("RegistrationSubmissionStatus.Pending")]
    pending = 1,
    [Description("RegistrationSubmissionStatus.Granted")]
    granted = 10,
    [Description("RegistrationSubmissionStatus.Refused")]
    refused = 20,
    [Description("RegistrationSubmissionStatus.Queried")]
    queried = 30,
    [Description("RegistrationSubmissionStatus.Cancelled")]
    cancelled = 40,
    [Description("RegistrationSubmissionStatus.Updated")]
    updated = 50
}