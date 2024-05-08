namespace EPR.RegulatorService.Frontend.Core.Enums;

using System.Text.Json.Serialization;
using System.ComponentModel;
using Converters;

[JsonConverter(typeof(OrganisationTypeConverter))]
public enum OrganisationType
{
    [Description("Direct Producer")]
    DirectProducer = 1,
    [Description("Compliance Scheme")]
    ComplianceScheme = 2,
}
