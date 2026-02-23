namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers;

    [TestClass]
    public class RejectRegistrationSubmissionViewModelTests
    {
        [TestMethod]
        public void RejectRegistrationSubmissionViewModel_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var viewModel = new RejectRegistrationSubmissionViewModel();

            // Assert
            Assert.AreEqual(Guid.Empty, viewModel.SubmissionId, "SubmissionId should be Guid.Empty by default.");
            Assert.IsNull(viewModel.RejectReason, "RejectReason should be null by default.");
            Assert.IsFalse(viewModel.IsResubmission, "IsResubmission should be false by default.");
        }

        [TestMethod]
        public void RejectRegistrationSubmissionViewModel_ShouldAllowSettingSubmissionId()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var viewModel = new RejectRegistrationSubmissionViewModel
            {
                // Act
                SubmissionId = submissionId
            };

            // Assert
            Assert.AreEqual(submissionId, viewModel.SubmissionId, "SubmissionId should match the assigned value.");
        }

        [TestMethod]
        public void RejectRegistrationSubmissionViewModel_ShouldAllowSettingRejectReason()
        {
            // Arrange
            string rejectReason = "This submission does not meet the required standards.";
            var viewModel = new RejectRegistrationSubmissionViewModel
            {
                // Act
                RejectReason = rejectReason
            };

            // Assert
            Assert.AreEqual(rejectReason, viewModel.RejectReason, "RejectReason should match the assigned value.");
        }

        [TestMethod]
        public void RejectRegistrationSubmissionViewModel_ShouldAllowNullRejectReason()
        {
            // Arrange
            var viewModel = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = "Initial reason"
            };

            // Act
            viewModel.RejectReason = null;

            // Assert
            Assert.IsNull(viewModel.RejectReason, "RejectReason should be null after setting it to null.");
        }

        [TestMethod]
        public void RejectReason_ShouldEnforceStringLengthValidation()
        {
            // Arrange
            string rejectReason = new('A', 401); // Exceeding max length of 400
            var viewModel = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = rejectReason
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.IsTrue(validationResults.Count != 0, "Validation results should contain errors.");
            var error = validationResults[0];
            Assert.AreEqual("Error.RejectReasonTooLong", error.ErrorMessage, "Error message should match the string length validation error.");
        }

        [TestMethod]
        public void RejectReason_ShouldAllowMaxLengthString()
        {
            // Arrange
            string rejectReason = new('A', 400); // Exactly max length
            var viewModel = new RejectRegistrationSubmissionViewModel
            {
                RejectReason = rejectReason
            };

            // Act
            var validationResults = ValidationHelper.ValidateModel(viewModel);

            // Assert
            Assert.AreEqual(0, validationResults.Count, "Validation results should not contain errors for a valid length string.");
        }
    }
}
