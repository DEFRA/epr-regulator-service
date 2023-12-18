namespace EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;

using System.Text.Json.Serialization;

public class CompanyUserInformation
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int PersonRoleId { get; set; }
    [JsonPropertyName("userEnrolments")]
    public List<CompanyEnrolments> UserEnrolments { get; set; }
    public bool? IsEmployee { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid ExternalId { get; set; }
}