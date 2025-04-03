namespace EPR.RegulatorService.Frontend.Core.Configs;

public class ReprocessorExporterFacadeApiConfig
{
    public const string ConfigSection = "ReprocessorExporterFacadeApi";
    
    public string DownstreamScope { get; set; } = string.Empty;

    public bool UseMockData { get; set; }
    
    public string BaseUrl { get; set; } = string.Empty;
    
    public Dictionary<string, string> Endpoints { get; set; } = new();
}