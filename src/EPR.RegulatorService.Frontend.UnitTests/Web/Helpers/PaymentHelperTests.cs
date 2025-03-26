namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers;

using Frontend.Web.Helpers;

[TestClass]
public class PaymentHelperTests
{
    [TestMethod]
    public void GetUpdatedTotalOutstanding_Should_Return_Zero()
    {
        // Arrange

        // Act
        decimal totalOutstanding = PaymentHelper.GetUpdatedTotalOutstanding(-500.00M);

        // Assert
        totalOutstanding.Should().Be(0);
    }

    [TestMethod]
    public void GetUpdatedTotalOutstanding_Should_Return_A_Positive_Decimal()
    {
        // Arrange
        decimal totalOutstanding = PaymentHelper.GetUpdatedTotalOutstanding(500.00M);

        // Assert
        totalOutstanding.Should().Be(500.00M);
    }
}