using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class QueryAccreditationSession
{
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required Guid AccreditationId { get; init; }
    public int? Year { get; set; }
    public required RegulatorTaskType TaskName { get; set; }
    public required Guid RegistrationId { get; set; }
}