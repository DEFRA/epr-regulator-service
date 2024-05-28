using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Core.Models.Registrations;

[ExcludeFromCodeCoverage]
public class Registration : AbstractSubmission
{
    public DateTime RegistrationDate { get; set; }
    public string RejectionComments { get; set; } = string.Empty;
    public string? CompaniesHouseNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? SubBuildingName { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? Locality { get; set; }
    public string? DependantLocality { get; set; }
    public string? Town { get; set; }
    public string? County { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
    public Guid OrganisationDetailsFileId { get; set; }
    public string OrganisationDetailsFileName { get; set; }
    public Guid CompanyDetailsFileId { get; set; }
    public string CompanyDetailsFileName { get; set; }
    public string CompanyDetailsBlobName { get; set; }
    public Guid? PartnershipFileId { get; set; }
    public string PartnershipFileName { get; set; }
    public string PartnershipBlobName { get; set; }
    public Guid? BrandsFileId { get; set; }
    public string BrandsFileName { get; set; }
    public string BrandsBlobName { get; set; }
}
