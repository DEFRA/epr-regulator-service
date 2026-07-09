namespace EPR.RegulatorService.Frontend.Core.Configs
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [ExcludeFromCodeCoverage]
    public class SubmissionFiltersConfig
    {
        public const string ConfigSection = "SubmissionFilters";

        public int[] Years { get; set; }

        public int[] OrgYears { get; set; }

        public string[] PomPeriods { get; set; }

        public string[] OrgPeriods { get; set; }

        public int[] ParseSubmissionYears(TimeProvider timeProvider)
        {
            var startYear = 2025;
            var addYears = true;

            var yearList = new List<int>{startYear};

            while (addYears)
            {
                if (timeProvider.GetUtcNow().Year == startYear)
                {
                    if (timeProvider.GetUtcNow().Month > 6)
                    {
                        yearList.Add(startYear + 1);
                    }

                    addYears = false;
                }
                else
                {
                    startYear++;
                    yearList.Add(startYear);
                }

            }
            return yearList.ToArray();
        }
    }
}
