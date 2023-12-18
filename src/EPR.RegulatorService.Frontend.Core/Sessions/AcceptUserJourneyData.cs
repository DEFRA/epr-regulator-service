using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Core.Sessions;

public class AcceptUserJourneyData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Guid OrganisationId { get; set; }
    public string ServiceRole { get; set; }
    public EndpointResponseStatus? ResponseStatus { get; set; }
}
