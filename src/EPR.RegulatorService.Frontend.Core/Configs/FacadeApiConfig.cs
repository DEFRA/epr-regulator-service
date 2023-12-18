namespace EPR.RegulatorService.Frontend.Core.Configs;

public class FacadeApiConfig
{
    public const string ConfigSection = "FacadeApi";
    
    public string DownstreamScope { get; set; } = string.Empty;

    public bool UseMockData { get; set; } = false;
    
    public string BaseUrl { get; set; } = string.Empty;
    
    public Dictionary<string, string> Endpoints { get; set; } = new();
}