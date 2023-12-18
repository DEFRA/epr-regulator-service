namespace EPR.RegulatorService.Frontend.Core.Models;

using System.Text.Json.Serialization;

public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string JobTitle { get; set; }
    public string TelephoneNumber { get; set; }
    public bool IsEmployeeOfOrganisation { get; set; }

    [JsonPropertyName("Enrolments")]
    public Enrolment Enrolment { get; set; }
    public string? TransferComments { get; set; }
}
