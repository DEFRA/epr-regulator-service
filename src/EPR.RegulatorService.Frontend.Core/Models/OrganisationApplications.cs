namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationApplications
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
    public PendingEnrollments Enrolments { get; set; } = new();
}