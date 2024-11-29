namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class ProducerPaymentDetailsViewModelTests
    {
        [TestMethod]
        public void ProducerPaymentDetailsViewModel_ValidModel_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                ApplicationProcessingFee = 100.00m,
                LateRegistrationFee = 50.00m,
                OnlineMarketplaceFee = 25.00m,
                SubsidiaryFee = 30.00m,
                SubsidiaryOnlineMarketPlaceFee = 40.00m,
                TotalChargeableItems = 245.00m,
                PreviousPaymentsReceived = 50.00m,
                TotalOutstanding = 195.00m,
                ProducerSize = "Medium",
                NumberOfSubsidiaries = 3,
                NumberOfSubsidiariesBeingOnlineMarketplace = 2,
                OfflinePayment = "200.00"
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_OfflinePayment_InvalidFormat_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "invalid-value" // Invalid format
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for invalid OfflinePayment format.");
            Assert.AreEqual("PaymentValidation.NonNumericCharacters", validationResults[0].ErrorMessage, "Expected the correct error message for invalid OfflinePayment format.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_OfflinePayment_Required_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = null // Required field
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for missing OfflinePayment.");
            Assert.AreEqual("PaymentValidation.TheAmountIsRequired", validationResults[0].ErrorMessage, "Expected the correct error message for missing OfflinePayment.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_OfflinePayment_ExceedsMaxValue_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "10000000.01" // Value exceeding the max value
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for OfflinePayment exceeding the maximum value.");
            Assert.AreEqual("PaymentValidation.ValueIsTooHigh", validationResults[0].ErrorMessage, "Expected the correct error message for OfflinePayment exceeding the maximum value.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_OfflinePayment_ZeroValue_ShouldHaveValidationError()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "0.00" // Value cannot be zero
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(1, validationResults.Count, "Expected one validation error for OfflinePayment being zero.");
            Assert.AreEqual("PaymentValidation.ValueCannotBeZero", validationResults[0].ErrorMessage, "Expected the correct error message for OfflinePayment being zero.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_OfflinePayment_Valid_ShouldHaveNoValidationErrors()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "500.00" // Valid value
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for valid OfflinePayment.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_EnsureTwoDecimalPlaces_ShouldFormatCorrectly()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "150" // No decimal point
            };

            // Act
            viewModel.EnsureTwoDecimalPlaces();

            // Assert
            Assert.AreEqual("150.00", viewModel.OfflinePayment, "Expected OfflinePayment to be formatted with two decimal places.");
        }

        [TestMethod]
        public void ProducerPaymentDetailsViewModel_EnsureTwoDecimalPlaces_WithDecimal_ShouldNotChange()
        {
            // Arrange
            var viewModel = new ProducerPaymentDetailsViewModel
            {
                OfflinePayment = "150.50" // Already has decimal places
            };

            // Act
            viewModel.EnsureTwoDecimalPlaces();

            // Assert
            Assert.AreEqual("150.50", viewModel.OfflinePayment, "Expected OfflinePayment to remain unchanged when it already has two decimal places.");
        }
    }
}
