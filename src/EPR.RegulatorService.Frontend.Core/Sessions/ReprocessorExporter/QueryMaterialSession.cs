namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class QueryMaterialSession
{
    public required string OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required Guid RegulatorApplicationTaskStatusId { get; init; }
    public required Guid RegistrationMaterialId { get; init; }
    public required string PagePath { get; set; }
}
