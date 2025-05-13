namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[TestClass]
public class ConfirmRegistrationRefusalViewModelTests
{
    private readonly string _submissionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    [TestMethod]
    public void ConfirmRegistrationRefusalViewModel_ValidModel_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = _submissionId,
            IsRegistrationRefusalConfirmed = true,
            RejectReason = "Reason for refusal"
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors for a valid model.");
    }

    [TestMethod]
    public void ConfirmRegistrationRefusalViewModel_NullSubmissionId_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = null, // Nullable field
            IsRegistrationRefusalConfirmed = true,
            RejectReason = "Reason for refusal"
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when SubmissionId is null.");
    }

    [TestMethod]
    public void ConfirmRegistrationRefusalViewModel_ShouldHaveValidationError_WhenIsRegistrationRefusalConfirmedIsNull()
    {
        // Arrange
        var viewModel = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = _submissionId,
            IsRegistrationRefusalConfirmed = null, // Required field
            RejectReason = "Reason for refusal"
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(1, validationResults.Count, "Expected one validation error for null IsRegistrationRefusalConfirmed.");
        Assert.AreEqual("ConfirmRegistrationRefusal.ErrorMessage", validationResults[0].ErrorMessage, "Expected the correct error message for IsRegistrationRefusalConfirmed being null.");
    }

    [TestMethod]
    public void ConfirmRegistrationRefusalViewModel_NullRejectReason_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = _submissionId,
            IsRegistrationRefusalConfirmed = true,
            RejectReason = null // Nullable field
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when RejectReason is null.");
    }

    [TestMethod]
    public void ConfirmRegistrationRefusalViewModel_RejectReasonEmpty_ShouldHaveNoValidationErrors()
    {
        // Arrange
        var viewModel = new ConfirmRegistrationRefusalViewModel
        {
            SubmissionId = _submissionId,
            IsRegistrationRefusalConfirmed = true,
            RejectReason = "" // Empty string
        };

        // Act
        var validationResults = ValidationHelper.ValidateModel(viewModel);

        // Assert
        Assert.AreEqual(0, validationResults.Count, "Expected no validation errors when RejectReason is an empty string.");
    }
}