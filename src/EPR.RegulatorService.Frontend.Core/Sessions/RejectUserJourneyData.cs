using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class RejectUserJourneyData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string ApprovedUserFirstName { get; set; }
    public string ApprovedUserLastName { get; set; }
    public Guid OrganisationId { get; set; }
    public string ServiceRole { get; set; }
    public string Decision { get; set; }    
    public EndpointResponseStatus? ResponseStatus { get; set; }
}