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
        public void GetPanelTitle_WhenRegistrationNumberIsProvided_ReturnsRegistrationReferenceNumberTitle()
        {
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                RegistrationReferenceNumber = "REGNUM"
            };

            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelTitle_WhenRegistrationNumberIsNotProvided_ReturnsApplicationReferenceNumber()
        {
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                RegistrationReferenceNumber = string.Empty
            };

            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);
            Assert.AreEqual("RegistrationSubmissionDetails.ApplicationReferenceNumber", result);
        }

        [Ignore]
        [TestMethod]
        public void GetPanelTitle_WhenStatusIsGranted_ReturnsRegistrationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber", result);
        }

        [Ignore]
        [TestMethod]
        public void GetPanelTitle_WhenStatusIsNotGranted_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Pending // or any other status except 'granted'
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.ApplicationReferenceNumber", result);
        }

        [Ignore]
        [TestMethod]
        public void GetPanelContent_WhenStatusIsGranted_ReturnsRegistrationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                RegistrationReferenceNumber = "REG123"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("REG123", result);
        }

        [Ignore]
        [TestMethod]
        public void GetPanelContent_WhenStatusIsNotGranted_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Pending,
                ApplicationReferenceNumber = "APP456"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("APP456", result);
        }
    }
}
