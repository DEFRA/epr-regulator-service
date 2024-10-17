
using System.ComponentModel;

namespace EPR.RegulatorService.Frontend.Core.Enums;

public enum RegistrationSubmissionStatus
{
    [Description("Not specified")]
    none = 0,
    [Description("Pending")]
    pending = 1,
    [Description("Granted")]
    granted = 10,
    [Description("Refused")]
    refused = 20,
    [Description("Queried")]
    queried = 30,
    [Description("Cancelled")]
    cancelled = 40,
    [Description("Updated")]
    updated = 50
}