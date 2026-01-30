namespace EPR.RegulatorService.Frontend.Core.Enums;

using System.ComponentModel;

public enum RegistrationJourneyType
{
    [Description("RegistrationJourneyType.NotSpecified")]
    Unknown,

    [Description("RegistrationJourneyType.CsoLegacy")]
    CsoLegacy,

    [Description("RegistrationJourneyType.CsoLargeProducer")]
    CsoLargeProducer,

    [Description("RegistrationJourneyType.CsoSmallProducer")]
    CsoSmallProducer,

    [Description("RegistrationJourneyType.DirectLargeProducer")]
    DirectLargeProducer,

    [Description("RegistrationJourneyType.DirectSmallProducer")]
    DirectSmallProducer
}