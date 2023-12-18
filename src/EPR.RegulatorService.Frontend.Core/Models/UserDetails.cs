using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models;

public class UserDetails
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = null!;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = null!;

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("roleInOrganisation")]
    public string RoleInOrganisation { get; set; } = null!;

    [JsonPropertyName("enrolmentStatus")]
    public string EnrolmentStatus { get; set; } = null!;

    [JsonPropertyName("serviceRole")]
    public string ServiceRole { get; set; } = null!;

    [JsonPropertyName("service")]
    public string Service { get; set; } = null!;

    [JsonPropertyName("serviceRoleId")]
    public int ServiceRoleId { get; set; }

    [JsonPropertyName("organisations")]
    public List<OrganisationResponse> Organisations { get; set; } = null!;
}
