namespace EPR.RegulatorService.Frontend.Core.Models.Registrations;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RegistrationSubmission
{
    public Guid SubmissionId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? Decision { get; set; } = string.Empty;
    public bool IsResubmission { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid? ComplianceSchemeId { get; set; }
    public string? OrganisationName { get; set; }
    public string? OrganisationReference { get; set; }
    public string? CompaniesHouseNumber { get; set; }
    public string? OrganisationType { get; set; }
    public string? ProducerType { get; set; }
    public Guid? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? ServiceRole { get; set; }
    public Guid FileId { get; set; }
    public string? SubmissionPeriod { get; set; }
    public string? Comments { get; set; }
    public Guid CompanyDetailsFileId { get; set; }
    public string CompanyDetailsFileName { get; set; }
    public Guid? PartnershipFileId { get; set; }
    public string? PartnershipFileName { get; set; }
    public Guid? BrandsFileId { get; set; }
    public string? BrandsFileName { get; set; }
}