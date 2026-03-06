namespace EPR.RegulatorService.Frontend.Core.Configs
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [ExcludeFromCodeCoverage]
    public class SubmissionFiltersConfig
    {
        public const string ConfigSection = "SubmissionFilters";

        /// <summary>
        /// Comma separated list of years to show in filters
        /// </summary>
        public string? SubmissionYears { get; set; }

        public int[] Years { get; set; }

        public int[] OrgYears { get; set; }

        public string[] PomPeriods { get; set; }

        public string[] OrgPeriods { get; set; }

        public int[] ParseSubmissionYears() =>
            SubmissionYears?.Split(",").Select(year =>
            {
                if (!int.TryParse(year, CultureInfo.InvariantCulture, out int parsedYear))
                {
                    throw new Exception($"Invalid year in {ConfigSection}:{nameof(SubmissionYears)} '{year}'");
                }
                return parsedYear;
            }).ToArray() ?? [];
    }
}
