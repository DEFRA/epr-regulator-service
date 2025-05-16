namespace EPR.RegulatorService.Frontend.Core.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SubmissionFiltersConfig
    {
        public const string ConfigSection = "SubmissionFilters";

        public int[] Years { get; set; }

        public string[] PomPeriods { get; set; }

        public string[] OrgPeriods { get; set; }
    }
}
