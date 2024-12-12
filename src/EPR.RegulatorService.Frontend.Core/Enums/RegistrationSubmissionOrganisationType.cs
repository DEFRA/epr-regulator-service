using System.ComponentModel;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Converters;

namespace EPR.RegulatorService.Frontend.Core.Enums;

[JsonConverter(typeof(RegistrationSubmissionOrganisationTypeConverter))]
public enum RegistrationSubmissionOrganisationType
{
    [Description("RegistrationSubmissionOrganisationType.NotSpecified")]
    none = 0,
    [Description("RegistrationSubmissionOrganisationType.ComplianceScheme")]
    compliance = 1,
    [Description("RegistrationSubmissionOrganisationType.LargeProducer")]
    large = 10,
    [Description("RegistrationSubmissionOrganisationType.SmallProducer")]
    small = 20
}