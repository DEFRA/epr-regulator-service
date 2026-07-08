namespace EPR.RegulatorService.Frontend.Core.Services
{
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.Configs;

    using Microsoft.Extensions.Options;

    public class SubmissionFilterConfigService(
        IOptions<SubmissionFiltersConfig> submissionFiltersConfig,
        IOptions<PackagingDataSubmissionsConfig> packagingDataSubmissionsConfig,
        TimeProvider timeProvider) : ISubmissionFilterConfigService
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

            var startingYear2 = 2026;
            var window1 = "January to June ";
            var window2 = "July to December ";
            var window3 = "January to December ";
            if (timeProvider.GetUtcNow().Year >= startingYear2)
            {

                var addYearData = true;
                while (addYearData)
                {
                    if (timeProvider.GetUtcNow().Year == startingYear2)
                    {
                        if (timeProvider.GetUtcNow().Month > 6)
                        {
                            filteredPeriods = filteredPeriods.Append(window1 + startingYear2).ToArray();
                            filteredYears = filteredYears.Append(startingYear2).ToArray();
                        }
                        addYearData = false;
                    }

                    if (timeProvider.GetUtcNow().Year > startingYear2)
                    {
                        filteredPeriods = filteredPeriods
                            .Append(window1 + startingYear2)
                            .Append(window2 + startingYear2)
                            .Append(window3 + startingYear2)
                            .ToArray();
                        filteredYears = filteredYears.Append(startingYear2).ToArray();
                    }

                    startingYear2++;
                }
            }

            return (filteredYears, filteredPeriods);
        }
    }
}
