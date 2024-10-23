namespace EPR.RegulatorService.Frontend.Core.Enums;

using System.ComponentModel;

public enum ServiceRole
{
    [Description("ServiceRole.ApprovedPerson")]
    ApprovedPerson = 1,

    [Description("ServiceRole.DelegatedPerson")]
    DelegatedPerson = 2,

    [Description("ServiceRole.ProducerOther")]
    ProducerOther = 3,

    [Description("ServiceRole.RegulatorAdmin")]
    RegulatorAdmin = 4,

    [Description("ServiceRole.RegulatorBasic")]
    RegulatorBasic = 5
}