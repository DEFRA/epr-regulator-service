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
        decimal totalOutstanding = PaymentHelper.GetUpdatedTotalOutstanding(-500.00M, true);

        // Assert
        totalOutstanding.Should().Be(0);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetUpdatedTotalOutstanding_Should_Return_A_Positive_Decimal(bool toggle)
    {
        // Arrange
        decimal totalOutstanding = PaymentHelper.GetUpdatedTotalOutstanding(500.00M, toggle);

        // Assert
        totalOutstanding.Should().Be(500.00M);
    }

    [TestMethod]
    public void GetUpdatedTotalOutstanding_Should_Return_A_Negative_Decimal_When_Toggle_Is_False()
    {
        // Arrange
        decimal totalOutstanding = PaymentHelper.GetUpdatedTotalOutstanding(-500.00M, false);

        // Assert
        totalOutstanding.Should().Be(-500.00M);
    }
}