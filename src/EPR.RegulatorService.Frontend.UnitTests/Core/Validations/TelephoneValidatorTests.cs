using EPR.RegulatorService.Frontend.Core.Validations;

namespace EPR.RegulatorService.Frontend.UnitTests;

[TestClass]
public class TelephoneValidatorTests
{
    [TestMethod]
    [DataRow("07489621402", true)]    
    [DataRow("020 1212 1212", true)]
    [DataRow("078 1212 1212", true)]
    [DataRow("78 1212 1212", true)]
    [DataRow("+44 078 1212 1212", true)]
    [DataRow("+44 78 1212 1212", true)]
    [DataRow("(+44) 78 1212 1212", true)]
    [DataRow("0044 078 1212 1212", true)]
    [DataRow("02012121212", true)]
    [DataRow("07812121212", true)]
    [DataRow("7812121212", true)]
    [DataRow("+4407812121212", true)]
    [DataRow("+447812121212", true)]
    [DataRow("004407812121212", true)]
    [DataRow("+49 30 901820", true)]
    [DataRow("+34919931307", true)]
    [DataRow("05815985", false)]
    [DataRow("020 1212 121", false)]
    [DataRow("020 1212 121", false)]
    [DataRow("078 1212 121A", false)]
    [DataRow("", false)]
    [DataRow("a", false)]
    [DataRow("800 890 567sad", false)]
    [DataRow("800 890 567123", false)]
    [DataRow("asd800 890 567", false)]
    [DataRow("123800 890 567", false)]
    [DataRow("07812121212!!", false)]
    [DataRow("..07812121212", false)]
    [DataRow("!@£$%800 890 567", false)]
    [DataRow("072121212^&*()_+", false)]
    [DataRow("0721^&*()_+2121", false)]
    [DataRow("078 1(212 12)1A", false)]
    public void WhenTheTelephoneNumber_AndTheValudation_ShouldReturnTheExpectedValidation(string phoneNumber, bool expectedBool)
    {
        // Arrange

        // Act
        bool result = TelephoneNumberValidator.IsValid(phoneNumber);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expected: expectedBool, actual: result);
    }
}