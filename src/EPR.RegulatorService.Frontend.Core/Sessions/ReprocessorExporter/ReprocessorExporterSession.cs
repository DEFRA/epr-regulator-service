namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class ReprocessorExporterSession
{
    public ApplicationUpdateSession? ApplicationUpdateSession { get; set; }
    public RegistrationStatusSession? RegistrationStatusSession { get; set; }
    public AccreditationStatusSession? AccreditationStatusSession { get; set; }
}