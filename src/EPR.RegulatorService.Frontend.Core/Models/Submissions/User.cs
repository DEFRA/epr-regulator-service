using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

[ExcludeFromCodeCoverage]
public class User
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;

    public string ServiceRole { get; set; } = string.Empty;
}