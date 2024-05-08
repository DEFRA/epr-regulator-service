namespace EPR.RegulatorService.Frontend.UnitTests.Core.Extensions;

using Frontend.Core.Extensions;

[TestClass]
public class BoolExtensionMethodsTests
{
    [TestMethod]
    public void ToYesNo_WhenTrue_ShouldReturnYes()
    {
        // Arrange
        bool testValue = true;

        // Act
        var result = testValue.ToYesNo();

        // Assert
        result.Should().Be("Yes", "because calling ToYesNo on true should return 'Yes'");
    }

    [TestMethod]
    public void ToYesNo_WhenFalse_ShouldReturnNo()
    {
        // Arrange
        bool testValue = false;

        // Act
        var result = testValue.ToYesNo();

        // Assert
        result.Should().Be("No", "because calling ToYesNo on false should return 'No'");
    }
}