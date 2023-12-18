using EPR.RegulatorService.Frontend.Core.Models;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.Core.Configs
{
    public class TransferOrganisationConfig
    {
        public const string ConfigSection = "Regulators";
        public string Organisations { get; set; }
        public RegulatorOrganisations Data => JsonSerializer.Deserialize<RegulatorOrganisations>(Organisations);
    }
}