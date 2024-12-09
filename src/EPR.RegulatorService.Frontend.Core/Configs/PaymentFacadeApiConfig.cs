namespace EPR.RegulatorService.Frontend.Core.Configs;

public class PaymentFacadeApiConfig
{
    public const string ConfigSection = "PaymentFacadeApi";

    public string DownstreamScope { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public Dictionary<string, string> Endpoints { get; set; } = new();
}