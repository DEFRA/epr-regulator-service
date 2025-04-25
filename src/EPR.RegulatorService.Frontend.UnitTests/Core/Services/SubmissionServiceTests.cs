namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services
{
    using EPR.RegulatorService.Frontend.Core.Configs;
    using EPR.RegulatorService.Frontend.Core.Services;

    using FluentAssertions;

    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SubmissionServiceTests
    {
        private SubmissionService _submissionService;

        private void CreateService(
            int[] years,
            string[] pomPeriods,
            int activeYear,
            int startingYear,
            int yearsAhead)
        {
            var submissionFiltersConfig = Options.Create(new SubmissionFiltersConfig
            {
                Years = years,
                PomPeriods = pomPeriods,
                OrgPeriods = [] // Not used in this service
            });

            var packagingDataSubmissionsConfig = Options.Create(new PackagingDataSubmissionsConfig
            {
                ActiveYear = activeYear,
                StartingYear = startingYear,
                YearsAhead = yearsAhead
            });

            _submissionService = new SubmissionService(submissionFiltersConfig, packagingDataSubmissionsConfig);
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_ShouldReturnExpectedYearsAndPeriods()
        {
            // Arrange
            CreateService(
                years: [2023, 2024, 2025, 2026],
                pomPeriods: [
                    "January to June 2023",
                    "July to December 2024",
                    "January to December 2025",
                    "January to December 2026"
                ],
                activeYear: 2025,
                startingYear: 2023,
                yearsAhead: 0
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEquivalentTo([2023, 2024, 2025]);
            filteredPeriods.Should().BeEquivalentTo(
                "January to June 2023",
                "July to December 2024",
                "January to December 2025"
            );
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_ShouldExcludeYearsOutsideRange()
        {
            // Arrange
            CreateService(
                years: [2022, 2023, 2026],
                pomPeriods: [
                    "January to June 2022",
                    "July to December 2023",
                    "January to December 2026"
                ],
                activeYear: 2024,
                startingYear: 2023,
                yearsAhead: 0
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEquivalentTo([2023]);
            filteredPeriods.Should().BeEquivalentTo(
                "July to December 2023"
            );
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_ShouldReturnEmpty_WhenNoDataWithinRange()
        {
            // Arrange
            CreateService(
                years: [2020, 2021],
                pomPeriods: [
                    "January to June 2020",
                    "July to December 2021"
                ],
                activeYear: 2025,
                startingYear: 2023,
                yearsAhead: 0
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEmpty();
            filteredPeriods.Should().BeEmpty();
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_ShouldIncludeYearsUpToYearsAhead()
        {
            // Arrange
            CreateService(
                years: [2023, 2024, 2025, 2026, 2027],
                pomPeriods: [
                    "January to June 2023",
                    "July to December 2026",
                    "January to December 2027"
                ],
                activeYear: 2025,
                startingYear: 2023,
                yearsAhead: 2 // Includes up to 2027
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEquivalentTo([2023, 2024, 2025, 2026, 2027]);
            filteredPeriods.Should().BeEquivalentTo(
                "January to June 2023",
                "July to December 2026",
                "January to December 2027"
            );
        }
    }
}
