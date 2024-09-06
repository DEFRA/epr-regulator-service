using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models;

[ExcludeFromCodeCoverage]
public class RegulatorUserDetailsUpdateResponse
{
    public bool HasUserDetailsChangeAccepted { get; set; } = false;
    public bool HasUserDetailsChangeRejected { get; set; } = false;

    public ChangeHistoryModel? ChangeHistory { get; set; }
}
