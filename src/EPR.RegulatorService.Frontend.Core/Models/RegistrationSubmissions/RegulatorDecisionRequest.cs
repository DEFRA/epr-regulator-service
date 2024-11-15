namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegulatorDecisionRequest
{
    public Guid OrganisationId { get; set; }

    public string Status { get; set; }

    public Guid SubmissionId { get; set; }

    public string? Comments { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CountryName CountryName { get; set; }

    public RegistrationSubmissionType RegistrationSubmissionType { get; set; }

    public string? TwoDigitYear { get; set; }

    public string? OrganisationAccountManagementId { get; set; }

    // New property for the decision date to be added here as well as the Facade
}