namespace EPR.RegulatorService.Frontend.Core.Models
{
    using System.Text.Json.Serialization;

    public class RegulatorOrganisations
    {
        [JsonPropertyName("data")]
        public List<RegulatorOrganisation> Organisations { get; set; }
    }
}