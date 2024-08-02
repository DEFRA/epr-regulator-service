namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RegistrationDetailsViewModel
{
    public string OrganisationName { get; set; }
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
    public string OrganisationType { get; set; }
    public string OrganisationReferenceNumber { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public string FormattedTimeAndDateOfSubmission { get; set; }
    public Guid SubmissionId { get; set; }
    public string SubmittedBy { get; set; }
    public string SubmissionPeriod { get; set; }
    public string DeclaredBy { get; set; }
    public string AccountRole { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public string PowerBiLogin { get; set; }
    public bool IsResubmission { get; set; }
    public string RejectionReason { get; set; }
    public string PreviousRejectionReason { get; set; }
    public string OrganisationDetailsFileName { get; set; }
    public Guid OrganisationDetailsFileId { get; set; }
    public string PartnershipDetailsFileName { get; set; }
    public Guid? PartnershipDetailsFileId { get; set; }
    public string BrandDetailsFileName { get; set; }
    public Guid? BrandDetailsFileId { get; set; }   
}