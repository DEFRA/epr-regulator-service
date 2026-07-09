namespace EPR.RegulatorService.Frontend.UnitTests.Core.Configs
{
    using EPR.RegulatorService.Frontend.Core.Configs;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    [TestClass]
    public class SubmissionFiltersConfigTests
    {
        private readonly SubmissionFiltersConfig _config = new()
        {
            Years = [],
            OrgYears = [],
            PomPeriods = [],
            OrgPeriods = []
        };

        private Mock<TimeProvider> CreateTimeProvider(int year, int month)
        {
            var mock = new Mock<TimeProvider>();
            mock.Setup(tp => tp.GetUtcNow())
                .Returns(new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero));
            return mock;
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2025AndMonthIsBeforeJuly_ShouldReturn2025Only()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2025, 6);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025]);
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2025AndMonthIsAfterJune_ShouldReturn2025And2026()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2025, 7);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025, 2026]);
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2026AndMonthIsBeforeJuly_ShouldReturn2025And2026()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2026, 3);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025, 2026]);
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2026AndMonthIsAfterJune_ShouldReturn2025To2027()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2026, 9);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025, 2026, 2027]);
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2027AndMonthIsBeforeJuly_ShouldReturn2025To2027()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2027, 2);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025, 2026, 2027]);
        }

        [TestMethod]
        public void ParseSubmissionYears_WhenCurrentYearIs2027AndMonthIsAfterJune_ShouldReturn2025To2028()
        {
            // Arrange
            var timeProvider = CreateTimeProvider(2027, 10);

            // Act
            var result = _config.ParseSubmissionYears(timeProvider.Object);

            // Assert
            result.Should().BeEquivalentTo([2025, 2026, 2027, 2028]);
        }
    }
}
