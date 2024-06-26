namespace EPR.RegulatorService.Frontend.Web.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SubmissionFiltersOptions
    {
        public const string ConfigSection = "SubmissionFilters";

        public int[] Years { get; set; }

        public string[] PomPeriods {  get; set; }

        public string[] OrgPeriods {  get; set; }
    }
}
