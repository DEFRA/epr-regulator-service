using EPR.RegulatorService.Frontend.Web.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Extensions;

[TestClass]
public class DecimalExtensionsTests
{
    [TestMethod]
    [DataRow(null, "")]
    [DataRow(100.00, "100")]
    [DataRow(50000.00, "50,000")]
    [DataRow(1234.50, "1,234.5")]
    [DataRow(1234.56, "1,234.56")]
    public void ToDisplayNumber_WhenNullableDecimal_ShouldReturnFormattedString(double? number, string expectedText)
    {
        // Arrange  
        decimal? decimalValue = number.HasValue ? (decimal?)number.Value : null;

        // Act  
        string result = decimalValue.ToDisplayNumber();

        // Assert  
        result.Should().Be(expectedText);
    }

    [TestMethod]
    [DataRow(100.00, "100")]
    [DataRow(50000.00, "50,000")]
    [DataRow(1234.50, "1,234.5")]
    [DataRow(1234.56, "1,234.56")]
    public void ToDisplayNumber_WhenNonNullableDecimal_ShouldReturnFormattedString(double number, string expectedText)
    {
        // Arrange  
        decimal? decimalValue = (decimal?)number;

        // Act
        string result = decimalValue.ToDisplayNumber();

        // Assert
        result.Should().Be(expectedText);
    }
}