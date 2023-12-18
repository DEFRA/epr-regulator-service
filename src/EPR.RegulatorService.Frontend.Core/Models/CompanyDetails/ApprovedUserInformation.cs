namespace EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;

public class ApprovedUserInformation
{
    public List<CompanyUserInformation> ApprovedUsers { get; set; }
    public string OrganisationName { get; set; }
    public Guid OrganisationExternalId { get; set; }
}