using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class QueryMaterialSession
{
    public required Guid RegistrationId { get; init; }
    public string? OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required Guid RegulatorApplicationTaskStatusId { get; init; }
    public required Guid RegistrationMaterialId { get; init; }
    public required RegulatorTaskStatus TaskStatus { get; init; }
    public required RegulatorTaskType TaskName { get; set; }
    public required string PagePath { get; set; }
}
