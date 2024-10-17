namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Web.Helpers;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReferenceNumberPanelHelperTests
    {
        [TestMethod]
        public void GetPanelTitle_WhenStatusIsGranted_ReturnsRegistrationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.granted
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelTitle_WhenStatusIsNotGranted_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.pending // or any other status except 'granted'
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.ApplicationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelContent_WhenStatusIsGranted_ReturnsRegistrationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.granted,
                RegistrationReferenceNumber = "REG123"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("REG123", result);
        }

        [TestMethod]
        public void GetPanelContent_WhenStatusIsNotGranted_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.pending,
                ApplicationReferenceNumber = "APP456"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("APP456", result);
        }
    }
}
