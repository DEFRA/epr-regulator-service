namespace EPR.RegulatorService.Frontend.Web.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class PackagingDataSubmissionsOptions
    {
        public const string ConfigSection = "BehaviourManagement:PomSubmissionFilters";

        //public bool Show2025SubmissionYearAndPeriods { get; set; }

        public int ActiveYear { get; set; }
        public int YearsBefore { get; set; }
        public int YearsAhead { get; set; }
    }
}
