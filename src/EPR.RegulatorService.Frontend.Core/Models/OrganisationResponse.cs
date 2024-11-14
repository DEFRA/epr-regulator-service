using System.Text.Json.Serialization;

namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("organisationRole")]
    public string OrganisationRole { get; set; } = null!;

    [JsonPropertyName("organisationType")]
    public string OrganisationType { get; set; } = null!;

    [JsonPropertyName("nationId")]
    public int? NationId { get; set; } = null!;
}