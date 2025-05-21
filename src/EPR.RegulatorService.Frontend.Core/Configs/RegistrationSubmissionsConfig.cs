namespace EPR.RegulatorService.Frontend.Core.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsConfig
    {
        public const string ConfigSection = "BehaviourManagement:RegistrationSubmissions";

        public bool Show2026RelevantYearFilter { get; set; }

        public int LateFeeCutOffMonth { get; set; }

        public int LateFeeCutOffDay { get; set; }
    }
}
