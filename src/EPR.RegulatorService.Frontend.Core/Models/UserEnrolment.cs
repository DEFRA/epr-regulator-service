using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models;

[ExcludeFromCodeCoverage]
public class UserEnrolment
{
    public bool IsApprovedUserAccepted { get; set; }
    public User User { get; set; }
}