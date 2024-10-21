namespace EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Enums;

public class RegistrationSubmissionOrganisationDetails
{
    public Guid OrganisationID { get; set; }
    public string OrganisationReference { get; set; }
    public string OrganisationName { get; set; }
    public RegistrationSubmissionOrganisationType OrganisationType { get; set; }
    public int NationID { get; set; }
    public int RegistrationYear { get; set; }
    public string Period { get; set; }
    public DateTime RegistrationDateTime { get; set; }
    public RegistrationSubmissionStatus RegistrationStatus { get; set; }
    public string? RegulatorComments { get; set; } = string.Empty;
    public string? ProducerComments { get; set; } = string.Empty;
    public string ApplicationReferenceNumber { get; set; } = String.Empty;
    public string? RegistrationReferenceNumber { get; set; } = String.Empty;
    public string CompaniesHouseNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? SubBuildingName { get; set; }
    public string? BuildingNumber { get; set; }
    public string Street { get; set; }
    public string Locality { get; set; }
    public string? DependentLocality { get; set; }
    public string Town { get; set; }
    public string County { get; set; }
    public string Country { get; set; }
    public string Postcode { get; set; }
}
