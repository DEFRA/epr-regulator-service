using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models;

public class UserDataResponse
{
    [JsonPropertyName("user")]
    public UserDetails UserDetails { get; set; } = null!;
}    
