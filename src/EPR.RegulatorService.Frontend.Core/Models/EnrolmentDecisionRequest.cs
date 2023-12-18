namespace EPR.RegulatorService.Frontend.Core.Models;

public class EnrolmentDecisionRequest
{
    public string OrganisationNumber { get; set; }
    public string OrganisationName { get; set; }
    public EmailDetails ApprovedUser { get; set; }
    public List<EmailDetails> DelegatedUsers { get; set; } = new();
    public string RejectionComments { get; set; }
    public string RegulatorRole { get; set; }
    public string Decision { get; set; }
}