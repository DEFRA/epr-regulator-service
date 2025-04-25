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
        private readonly SubmissionFiltersConfig _submissionFiltersConfig = submissionFiltersConfig.Value;
        private readonly PackagingDataSubmissionsConfig _packagingDataSubmissionsConfig = packagingDataSubmissionsConfig.Value;

        public (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods()
        {
            int[] submissionYears = _submissionFiltersConfig.Years;
            string[] submissionPeriods = _submissionFiltersConfig.PomPeriods;

            int activeYear = _packagingDataSubmissionsConfig.ActiveYear;
            int startingYear = _packagingDataSubmissionsConfig.StartingYear;
            int yearsAhead = _packagingDataSubmissionsConfig.YearsAhead;

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
