namespace EPR.RegulatorService.Frontend.Core.Configs;

public class ReprocessorExporterFacadeApiConfig
{
    public const string ConfigSection = "ReprocessorExporterFacadeApi";
    public string DownstreamScope { get; init; } = string.Empty;
    public int ApiVersion { get; init; }
    public bool UseMockData { get; init; }
    public string BaseUrl { get; init; } = string.Empty;
    public Dictionary<string, string> Endpoints { get; init; } = new();
}