namespace EPR.RegulatorService.Frontend.Core.Services
{
    using System;
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Configs;

    using Microsoft.Extensions.Options;

    public class SubmissionService(
        IOptions<SubmissionFiltersConfig> submissionFiltersConfig,
        IOptions<PackagingDataSubmissionsConfig> packagingDataSubmissionsConfig) : ISubmissionService
    {
        public (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods()
        {
            int[] submissionYears = submissionFiltersConfig.Value.Years;
            string[] submissionPeriods = submissionFiltersConfig.Value.PomPeriods;

            int activeYear = packagingDataSubmissionsConfig.Value.ActiveYear;
            int startingYear = packagingDataSubmissionsConfig.Value.StartingYear;
            int yearsAhead = packagingDataSubmissionsConfig.Value.YearsAhead;

            int latestYear = activeYear + yearsAhead;

            static bool IsBetween(int year, int min, int max) => year >= min && year <= max;

            int[] filteredYears = submissionYears
                .Where(year => IsBetween(year, startingYear, latestYear))
                .ToArray();

            string[] filteredPeriods = submissionPeriods
                .Where(sp => IsBetween(int.Parse(sp[^4..], CultureInfo.InvariantCulture), startingYear, latestYear))
                .ToArray();

            return (filteredYears, filteredPeriods);
        }
    }
}
