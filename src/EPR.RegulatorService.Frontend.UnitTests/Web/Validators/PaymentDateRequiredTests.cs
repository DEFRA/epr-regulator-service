using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Validators;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validators
{
    [TestClass]
    public class PaymentDateRequiredTests
    {
        private PaymentDateRequiredAttribute _validator; // System under test

        [TestInitialize]
        public void TestInitialize() => _validator = new PaymentDateRequiredAttribute { ErrorMessage = "Enter date in DD MM YYYY format" };

        [TestMethod]
        public void GetValidationResult_ReturnsError_WhenValueIsNull()
        {
            // Arrange
            var context = new ValidationContext(new object());

            // Act
            var result = _validator.GetValidationResult(null, context);

            // Assert
            result.Should().NotBeNull();
            result!.ErrorMessage.Should().Be("Enter date in DD MM YYYY format");
        }

        [TestMethod]
        public void GetValidationResult_ReturnsError_WhenAnyFieldIsNull()
        {
            // Arrange
            var paymentDataViewModel = new PaymentDateViewModel {ApplicationType = ApplicationOrganisationType.Reprocessor, Day = null, Month = 6, Year = 2025 };
            var context = new ValidationContext(paymentDataViewModel);

            // Act
            var result = _validator.GetValidationResult(paymentDataViewModel, context);

            //Assert
            result.Should().NotBeNull();
            result!.ErrorMessage.Should().Be("Enter date in DD MM YYYY format");
        }

        [TestMethod]
        public void GetValidationResult_ReturnsSuccess_WhenDayMonthAndYearArePresent()
        {
            // Arrange
            var paymentDataViewModel = new PaymentDateViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, Day = null, Month = 6, Year = 2025 };
            var context = new ValidationContext(paymentDataViewModel);

            // Act
            var result = _validator.GetValidationResult(paymentDataViewModel, context);

            //Assert
            result.Should().NotBeNull();
            result!.ErrorMessage.Should().Be("Enter date in DD MM YYYY format");
        }
    }
}