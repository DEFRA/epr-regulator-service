using EPR.RegulatorService.Frontend.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.Registrations;

[ExcludeFromCodeCoverage]
public class Registration
{
    public Guid RegistrationId { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string? Decision { get; set; } = string.Empty;
    public bool IsResubmission { get; set; }
    public string RejectionComments { get; set; } = string.Empty;
    public Guid OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public OrganisationType OrganisationType { get; set; }
    public string? OrganisationReference { get; set; }
    public Guid? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? ServiceRole { get; set; }
    public string CompanyDetailsField { get; set; }
    public string CompanyDetailsFileName { get; set; }
    public string PartnershipFileName { get; set; }
    public string PartnershipFileId { get; set; }
    public string BrandsFileName { get; set; }
    public string BrandsFileId { get; set; }
}
