namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class ReprocessorExporterSession
{
    public Guid RegistrationId { get; set; }
    public ApplicationUpdateSession? ApplicationUpdateSession { get; set; }
    public RegistrationStatusSession? RegistrationStatusSession { get; set; }
    public QueryMaterialSession? QueryMaterialSession { get; set; }
    public QueryRegistrationSession? QueryRegistrationSession { get; set; }
    public QueryAccreditationSession? QueryAccreditationSession { get; set; }
    public AccreditationStatusSession? AccreditationStatusSession { get; set; }
}