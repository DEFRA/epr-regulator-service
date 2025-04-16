using EPR.RegulatorService.Frontend.Web.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Extensions;

[TestClass]
public class DateTimeExtensionsTests
{
    [TestMethod]
    public void ToDisplayDate_WhenDateIsNull_ShouldReturnEmptyString()
    {
        // Act
        string result = DateTimeExtensions.ToDisplayDate(null);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("2025-01-01", "1 January 2025")] // Single digit day
    [DataRow("2025-01-31", "31 January 2025")] // Double digit day
    public void ToDisplayDate_WhenDateIsSet_ShouldReturnDateStringInExpectedFormat(string dateString, string expectedValue)
    {
        // Arrange
        var dateTime = DateTime.Parse(dateString);

        // Act
        string result = DateTimeExtensions.ToDisplayDate(dateTime);

        // Assert
        result.Should().Be(expectedValue);
    }

    [TestMethod]
    public void ToDisplayDateAndTime_WhenDateIsNull_ShouldReturnEmptyString()
    {
        // Act
        string result = DateTimeExtensions.ToDisplayDateAndTime(null);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    [DataRow("2025-01-01T09:00", "1 January 2025 at 9:00am")] // AM test
    [DataRow("2025-01-31T22:30", "31 January 2025 at 10:30pm")] // PM test
    public void ToDisplayDateAndTime_WhenDateIsSet_ShouldReturnDateStringInExpectedFormat(string dateString, string expectedValue)
    {
        // Arrange
        var dateTime = DateTime.Parse(dateString);

        // Act
        string result = DateTimeExtensions.ToDisplayDateAndTime(dateTime);

        // Assert
        result.Should().Be(expectedValue);
    }
}
