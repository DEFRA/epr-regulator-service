namespace EPR.RegulatorService.Frontend.Core.Services
{
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Configs;

    using Microsoft.Extensions.Options;

    public class SubmissionFilterConfigService(
        IOptions<SubmissionFiltersConfig> submissionFiltersConfig,
        IOptions<PackagingDataSubmissionsConfig> packagingDataSubmissionsConfig) : ISubmissionFilterConfigService
    {
        public (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods()
        {
            var submissionFiltersOptions = submissionFiltersConfig.Value;
            var packagingDataSubmissionOptions = packagingDataSubmissionsConfig.Value;

            int[] submissionYears = submissionFiltersOptions.Years;
            string[] submissionPeriods = submissionFiltersOptions.PomPeriods;

            int activeYear = packagingDataSubmissionOptions.ActiveYear;
            int startingYear = packagingDataSubmissionOptions.StartingYear;
            int yearsAhead = packagingDataSubmissionOptions.YearsAhead;

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
