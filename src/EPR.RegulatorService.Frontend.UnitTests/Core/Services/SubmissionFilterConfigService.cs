namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services
{
    using EPR.RegulatorService.Frontend.Core.Configs;

    using FluentAssertions;

    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class SubmissionFilterConfigService
    {
        private Frontend.Core.Services.SubmissionFilterConfigService _submissionFilterConfigService;

        private void CreateService(
            int[] years,
            string[] pomPeriods,
            int activeYear,
            int startingYear,
            int yearsAhead,
            DateTimeOffset currentDate)
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

            var mockTimeProvider = new Mock<TimeProvider>();
            mockTimeProvider.Setup(tp => tp.GetUtcNow()).Returns(currentDate);

            _submissionFilterConfigService = new Frontend.Core.Services.SubmissionFilterConfigService(
                submissionFiltersConfig,
                packagingDataSubmissionsConfig,
                mockTimeProvider.Object);
        }

        // This is to mimic the existing config - testing of future years uses this
        private void CreateServiceWithBaseConfig(DateTimeOffset currentDate) =>
            CreateService(
                years: [2023, 2024, 2025],
                pomPeriods: [
                    "January to June 2023",
                    "January to December 2024",
                    "January to December 2025"
                ],
                activeYear: 2025,
                startingYear: 2023,
                yearsAhead: 0,
                currentDate: currentDate);

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
                yearsAhead: 0,
                currentDate: new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

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
                yearsAhead: 0,
                currentDate: new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

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
                yearsAhead: 0,
                currentDate: new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

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
                yearsAhead: 2, // Includes up to 2027
                currentDate: new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
            );

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEquivalentTo([2023, 2024, 2025, 2026, 2027]);
            filteredPeriods.Should().BeEquivalentTo(
                "January to June 2023",
                "July to December 2026",
                "January to December 2027"
            );
        }

        // Tests to cover dynamic adding of filtering options based on current year

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIsBefore2026_ShouldNotAddDynamicPeriods()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().BeEquivalentTo([2023, 2024, 2025]);
            filteredPeriods.Should().NotContain(p => p.Contains("2026"));
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIs2026AndMonthIsBeforeJuly_ShouldNotAddDynamicPeriods()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().NotContain(2026);
            filteredPeriods.Should().NotContain(p => p.Contains("2026"));
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIs2026AndMonthIsAfterJune_ShouldAddJanuaryToJune2026Only()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().Contain(2026);
            filteredPeriods.Should().Contain("January to June 2026");
            filteredPeriods.Should().NotContain("July to December 2026");
            filteredPeriods.Should().NotContain("January to December 2026");
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIs2027AndMonthIsBeforeJuly_ShouldAddAllPeriodsFor2026Only()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2027, 3, 1, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().Contain(2026);
            filteredPeriods.Should().Contain("January to June 2026");
            filteredPeriods.Should().Contain("July to December 2026");
            filteredPeriods.Should().Contain("January to December 2026");
            filteredYears.Should().NotContain(2027);
            filteredPeriods.Should().NotContain(p => p.Contains("2027"));
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIs2027AndMonthIsAfterJune_ShouldAddAllPeriodsFor2026AndJanuaryToJune2027()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2027, 8, 1, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert
            filteredYears.Should().Contain(2026);
            filteredPeriods.Should().Contain("January to June 2026");
            filteredPeriods.Should().Contain("July to December 2026");
            filteredPeriods.Should().Contain("January to December 2026");
            filteredYears.Should().Contain(2027);
            filteredPeriods.Should().Contain("January to June 2027");
            filteredPeriods.Should().NotContain("July to December 2027");
            filteredPeriods.Should().NotContain("January to December 2027");
        }

        [TestMethod]
        public void GetFilteredSubmissionYearsAndPeriods_WhenCurrentYearIs2028AndMonthIsAfterJune_ShouldAddAllPeriodsFor2026And2027AndJanuaryToJune2028()
        {
            // Arrange
            CreateServiceWithBaseConfig(new DateTimeOffset(2028, 9, 1, 0, 0, 0, TimeSpan.Zero));

            // Act
            var (filteredYears, filteredPeriods) = _submissionFilterConfigService.GetFilteredSubmissionYearsAndPeriods();

            // Assert - 2026: all three windows
            filteredYears.Should().Contain(2026);
            filteredPeriods.Should().Contain("January to June 2026");
            filteredPeriods.Should().Contain("July to December 2026");
            filteredPeriods.Should().Contain("January to December 2026");
            // 2027: all three windows
            filteredYears.Should().Contain(2027);
            filteredPeriods.Should().Contain("January to June 2027");
            filteredPeriods.Should().Contain("July to December 2027");
            filteredPeriods.Should().Contain("January to December 2027");
            // 2028: January to June only
            filteredYears.Should().Contain(2028);
            filteredPeriods.Should().Contain("January to June 2028");
            filteredPeriods.Should().NotContain("July to December 2028");
            filteredPeriods.Should().NotContain("January to December 2028");
        }
    }
}
