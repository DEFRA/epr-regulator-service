namespace EPR.RegulatorService.Frontend.Core.Models.Submissions;

public class Organisation
{
    public Guid OrganisationId { get; set; }

    public string OrganisationName { get; set; } = string.Empty;

    public string OrganisationType { get; set; } = string.Empty;

    public string OrganisationReference { get; set; } = string.Empty;

    public User User { get; set; }
}