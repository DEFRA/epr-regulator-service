using System.ComponentModel.DataAnnotations;
using System.Globalization;

using EPR.RegulatorService.Frontend.Web.Attributes;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Attributes
{
    [TestClass]
    public class DateValidationAttributeTests
    {
        private const string InvalidDateErrorMessage = "Date of cancellation must be a real Date";
        private const string MissingDayErrorMessage = "Cancellation date must include a day";
        private const string MissingMonthErrorMessage = "Cancellation date must include a month";
        private const string MissingYearErrorMessage = "Cancellation date must include a year";
        private const string EmptyDateErrorMessage = "Enter a cancellation date";
        private const string InvalidRealDateErrorMessage = "Cancellation date must be a real date";
        private const string PastDateErrorMessage = "Cancellation date must be after 31 December 2024";

        private DateValidationAttribute _systemUnderTest = null!;

        [TestInitialize]
        public void Setup()
        {
            _systemUnderTest = new DateValidationAttribute(InvalidDateErrorMessage,
                                                            MissingDayErrorMessage,
                                                            MissingMonthErrorMessage,
                                                            MissingYearErrorMessage,
                                                            EmptyDateErrorMessage,
                                                            InvalidRealDateErrorMessage,
                                                            PastDateErrorMessage);

        }

        [TestMethod]
        [DataRow(2, 8, 2030)]
        [DataRow(02, 08, 2030)]
        public void Given_Valid_Date_When_Call_DateValidationAttribute_Then_Return_Success_ValidationResult(int? day, int? month, int? year)
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel { SubmissionId = Guid.NewGuid(), Day = day, Month = month, Year = year };
            var context = new ValidationContext(viewModel);

            // Act
            var result = _systemUnderTest.GetValidationResult(viewModel.Day, context);

            // Assert

            viewModel.CancellationDate.Should().NotBeNull();
            result.Should().Be(null);
            try
            {
               _ = DateTime.Parse(viewModel.CancellationDate.Value.ToString("d/M/yyyy", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Not expected DateTime format exception. But got an: {ex.Message}");
            }
 
        }

        [TestMethod]
        [DataRow(32, 8, 2030, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 13, 2030, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 01, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 12, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 999, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 10000, InvalidDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 1000, PastDateErrorMessage, "CancellationDate", false)]
        [DataRow(02, 03, 2023, PastDateErrorMessage, "CancellationDate", false)]
        [DataRow(null, 03, 10000, MissingDayErrorMessage, "CancellationDate", false)]
        [DataRow(02, null, 10000, MissingMonthErrorMessage, "CancellationDate", false)]
        [DataRow(02, 12, null, MissingYearErrorMessage, "CancellationDate", false)]
        [DataRow(null, null, null, EmptyDateErrorMessage, "CancellationDate", false)]
        [DataRow(12, 03, 2024, PastDateErrorMessage, "CancellationDate", true)]
        [DataRow(29, 02, 2025, InvalidRealDateErrorMessage, "CancellationDate", true)]
        public void Given_Invalid_Date_When_Call_DateValidationAttribute_Then_Return_Unsuccess_ValidationResult(int? day, int? month, int? year, string expectedErrorMessage, string memberName, bool cancellationDate)
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel { SubmissionId = Guid.NewGuid(), Day = day, Month = month, Year = year };
            var context = new ValidationContext(viewModel);

            // Act
            var result = _systemUnderTest.GetValidationResult(viewModel, context);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessage.Should().Be(expectedErrorMessage);
            result.MemberNames.ToList()[0].Should().Be(memberName);
            viewModel.CancellationDate.HasValue.Equals(cancellationDate);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Given_Invalid_ValidationContext_When_Call_DateValidationAttribute_Then_Should_Throws_An_InvalidOperationException()
        {
            // Arrange
            var viewModel = new CancelDateRegistrationSubmissionViewModel { SubmissionId = Guid.NewGuid(), Day = 1, };
            var context = new ValidationContext(new CancellationConfirmationViewModel());

            // Act
            var result = _systemUnderTest.GetValidationResult(viewModel.Day, context);

            result.Should().NotBeNull();
        }

    }
}