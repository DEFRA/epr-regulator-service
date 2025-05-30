namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers;

using AutoFixture;

using Frontend.Web.Helpers;

public class DateTimeHelpersTests
{
    private Fixture _fixture;

    [TestInitialize]
    public void Initialize()
    {
        _fixture = new Fixture();
    }

    [TestMethod]
    public void FormatTimeAndDateForSubmission_ShouldFormatCorrectlyForAMTime()
    {
        // Arrange
        var timeAndDateOfSubmission = _fixture.Create<DateTime>().Date.AddHours(9); // 9 AM
        var expectedFormat = $"{timeAndDateOfSubmission:h:mm}am on {timeAndDateOfSubmission:dd MMMM yyyy}";

        // Act
        var result = DateTimeHelpers.FormatTimeAndDateForSubmission(timeAndDateOfSubmission);

        // Assert
        result.Should().Be(expectedFormat);
    }

    [TestMethod]
    public void FormatTimeAndDateForSubmission_ShouldFormatCorrectlyForPMTime()
    {
        // Arrange
        var timeAndDateOfSubmission = _fixture.Create<DateTime>().Date.AddHours(15); // 3 PM
        var expectedFormat = $"{timeAndDateOfSubmission:h:mm}pm on {timeAndDateOfSubmission:dd MMMM yyyy}";

        // Act
        var result = DateTimeHelpers.FormatTimeAndDateForSubmission(timeAndDateOfSubmission);

        // Assert
        result.Should().Be(expectedFormat);
    }
}