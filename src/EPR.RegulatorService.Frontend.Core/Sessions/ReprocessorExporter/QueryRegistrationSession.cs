namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class QueryRegistrationSession
{
    public required string OrganisationName { get; init; }
    public string? SiteAddress { get; init; }
    public required Guid RegulatorRegistrationTaskStatusId { get; init; }
    public required Guid RegistrationId { get; init; }
    public required string PagePath { get; set; }
}