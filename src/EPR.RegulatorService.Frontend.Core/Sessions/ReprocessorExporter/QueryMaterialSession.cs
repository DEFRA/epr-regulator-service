namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public class QueryMaterialSession
{
    public required string OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public ApplicationOrganisationType ApplicationType { get; init; }
    public required Guid RegulatorApplicationTaskStatusId { get; init; }
    public required int RegistrationMaterialId { get; init; }
    public required string PagePath { get; set; }
}
