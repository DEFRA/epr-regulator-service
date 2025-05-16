using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Validators;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Validators
{
    [TestClass]
    public class PaymentDateIsPastDateAttributeTests
    {
        private PaymentDateIsPastDateAttribute _validator; // System under test

        [TestInitialize]
        public void TestInitialize() => _validator = new PaymentDateIsPastDateAttribute { ErrorMessage = "Test Message" };

        [TestMethod]
        public void IsValid_WhenViewModelIsCorrectTypePaymentDate_ShouldReturnSuccess()
        {
            // Arrange
            var context = new ValidationContext(new object());

            // Act
            var result = _validator.GetValidationResult(
                new PaymentMethodViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor },
                context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [TestMethod]
        public void IsValid_WhenDateFieldsAreMissing_ShouldReturnSuccess()
        {
            // Arrange
            var model = new PaymentDateViewModel
            {
                Day = null, Month = null, Year = null, ApplicationType = ApplicationOrganisationType.Reprocessor
            };
            var context = new ValidationContext(model);

            // Act
            var result = _validator.GetValidationResult(model, context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [TestMethod]
        public void IsValid_WhenDateIsInFuture_ShouldReturnError()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(1);
            var model = new PaymentDateViewModel
            {
                Day = futureDate.Day,
                Month = futureDate.Month,
                Year = futureDate.Year,
                ApplicationType = ApplicationOrganisationType.Reprocessor
            };
            var context = new ValidationContext(model);

            // Act
            var result = _validator.GetValidationResult(model, context);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
            result.ErrorMessage.Should().Be(_validator.ErrorMessage);
        }

        [TestMethod]
        public void IsValid_WhenDateIsToday_ShouldReturnSuccess()
        {
            // Arrange
            var today = DateTime.Today;
            var model = new PaymentDateViewModel
            {
                Day = today.Day,
                Month = today.Month,
                Year = today.Year,
                ApplicationType = ApplicationOrganisationType.Reprocessor
            };
            var context = new ValidationContext(model);

            // Act
            var result = _validator.GetValidationResult(model, context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [TestMethod]
        public void IsValid_WhenDateIsInPast_ShouldReturnSuccess()
        {
            // Arrange
            var pastDate = DateTime.Today.AddDays(-1);
            var model = new PaymentDateViewModel
            {
                Day = pastDate.Day,
                Month = pastDate.Month,
                Year = pastDate.Year,
                ApplicationType = ApplicationOrganisationType.Reprocessor
            };
            var context = new ValidationContext(model);

            // Act
            var result = _validator.GetValidationResult(model, context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [TestMethod]
        public void IsValid_WhenDateIsInvalid_ShouldReturnError()
        {
            // Arrange
            var model = new PaymentDateViewModel
            {
                Day = 31,
                Month = 2,
                Year = 2023,
                ApplicationType = ApplicationOrganisationType.Reprocessor
            };
            var context = new ValidationContext(model);

            // Act
            var result = _validator.GetValidationResult(model, context);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
            result.ErrorMessage.Should().Be(_validator.ErrorMessage);
        }
    }
}