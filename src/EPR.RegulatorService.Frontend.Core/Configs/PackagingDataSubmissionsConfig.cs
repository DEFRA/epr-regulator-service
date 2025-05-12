namespace EPR.RegulatorService.Frontend.Core.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class PackagingDataSubmissionsConfig
    {
        public const string ConfigSection = "BehaviourManagement:PomSubmissionFilters";

        public int ActiveYear { get; set; }

        public int StartingYear { get; set; }

        public int YearsAhead { get; set; }
    }
}
