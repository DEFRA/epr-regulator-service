namespace EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

public class ReprocessorExporterSession
{
    public ApplicationUpdateSession? ApplicationUpdateSession { get; set; }
    public RegistrationStatusSession? RegistrationStatusSession { get; set; }
    public QueryMaterialSession? QueryMaterialSession { get; set; }
    public QueryRegistrationSession? QueryRegistrationSession { get; set; }

}