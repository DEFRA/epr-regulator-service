namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;

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

    [TestMethod]
    public void PaymentDetailsViewModel_ValidModel_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "150.00"
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_OfflinePayment_InvalidFormat_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "invalid-value" // Invalid format
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for invalid OfflinePayment format.");
        Assert.AreEqual("PaymentValidation.NonNumericCharacters", validationResults[0].ErrorMessage, "Expected the correct error message for invalid OfflinePayment format.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_OfflinePayment_Required_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = null // Required field
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for missing OfflinePayment.");
        Assert.AreEqual("PaymentValidation.TheAmountIsRequired", validationResults[0].ErrorMessage, "Expected the correct error message for missing OfflinePayment.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_OfflinePayment_ExceedsMaxValue_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "10000000.01" // Value exceeding the max value
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for OfflinePayment exceeding the maximum value.");
        Assert.AreEqual("PaymentValidation.ValueIsTooHigh", validationResults[0].ErrorMessage, "Expected the correct error message for OfflinePayment exceeding the maximum value.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_OfflinePayment_ZeroValue_ShouldHaveValidationError()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "0.00" // Value cannot be zero
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for OfflinePayment being zero.");
        Assert.AreEqual("PaymentValidation.ValueCannotBeZero", validationResults[0].ErrorMessage, "Expected the correct error message for OfflinePayment being zero.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_OfflinePayment_Valid_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "150.00" // Valid value
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for valid OfflinePayment.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_EnsureTwoDecimalPlaces_ShouldFormatCorrectly()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "150" // No decimal point
        };

        // Act
        viewModel.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("150.00", viewModel.OfflinePayment, "Expected OfflinePayment to be formatted with two decimal places.");
    }

    [TestMethod]
    public void PaymentDetailsViewModel_EnsureTwoDecimalPlaces_WithDecimal_ShouldNotChange()
    {
        // Arrange
        var viewModel = new PaymentDetailsViewModel
        {
            ApplicationProcessingFee = 100.00m,
            OnlineMarketplaceFee = 50.00m,
            SubsidiaryFee = 25.00m,
            PreviousPaymentsReceived = 50.00m,
            OfflinePayment = "150.50" // Already has decimal places
        };

        // Act
        viewModel.EnsureTwoDecimalPlaces();

        // Assert
        Assert.AreEqual("150.50", viewModel.OfflinePayment, "Expected OfflinePayment to remain unchanged when it already has two decimal places.");
    }
}
