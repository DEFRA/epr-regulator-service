namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class PaymentDetailsViewModelTests
{
    [TestMethod]
    public void EnsureTwoDecimalPlaces_IntegerValue_AppendsTwoDecimalPlaces()
    {
        // Arrange
        var instance = new PaymentDetailsViewModel { OfflinePayment = "10" };

        // Act
        instance.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("10.00", instance.OfflinePayment);
    }

    [TestMethod]
    public void EnsureTwoDecimalPlaces_SingleDecimal_AppendsZero()
    {
        // Arrange
        var instance = new PaymentDetailsViewModel { OfflinePayment = "10.4" };

        // Act
        instance.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("10.40", instance.OfflinePayment);
    }

    [TestMethod]
    public void EnsureTwoDecimalPlaces_TwoDecimalPlaces_RetainsValue()
    {
        // Arrange
        var instance = new PaymentDetailsViewModel { OfflinePayment = "10.45" };

        // Act
        instance.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("10.45", instance.OfflinePayment);
    }

    [TestMethod]
    public void EnsureTwoDecimalPlaces_InvalidDecimalValue_DoesNotChangeOfflinePayment()
    {
        // Arrange
        var instance = new PaymentDetailsViewModel { OfflinePayment = "invalid" };

        // Act
        instance.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("invalid", instance.OfflinePayment); // Expecting it to remain unchanged
    }
}
