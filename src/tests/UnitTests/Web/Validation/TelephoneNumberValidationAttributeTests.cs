using EPR.RegulatorService.Frontend.Web.Controllers.Attributes;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validation;

[TestClass]
public class TelephoneNumberValidationAttributeTests
{
    private readonly TelephoneNumberValidationAttribute _telephoneNumberValidationAttribute = new();

    [TestMethod]
    [DataRow("020 1212 1212", true)]
    [DataRow(null, false)]
    public void IsValid_WhenPassedValidNumber_ReturnsTrue(string? number, bool expectedResult)
    {
        // Act
        bool result = _telephoneNumberValidationAttribute.IsValid(number);

        // Assert
        result.Should().Be(expectedResult);
    }
}