using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models;

[ExcludeFromCodeCoverage]
public class UserDetailsChangeModel
{
    /// <summary>
    /// User FirstName
    /// </summary>
    public string FirstName { get; init; }

    /// <summary>
    /// User LastName
    /// </summary>
    public string LastName { get; init; }

    /// <summary>
    /// User Jobtitle in an organisation
    /// </summary>
    public string? JobTitle { get; init; }
}
