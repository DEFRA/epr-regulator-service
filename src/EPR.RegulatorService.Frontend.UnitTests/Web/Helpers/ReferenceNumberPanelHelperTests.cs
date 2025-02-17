namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers
{
    using System.Text;

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
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                RegistrationReferenceNumber = "REGNUM"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelTitle_WhenRegistrationNumberIsNotProvided_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                RegistrationReferenceNumber = string.Empty
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.ApplicationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelTitle_WhenStatusIsGranted_ReturnsRegistrationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                RegistrationReferenceNumber = "REG123",
                Status = RegistrationSubmissionStatus.None
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelTitle_WhenStatusIsGrantedAndIsResubmission_ReturnsBothTitles()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                IsResubmission = true
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelTitle(model);

            // Assert
            Assert.AreEqual("RegistrationSubmissionDetails.RegistrationReferenceNumber|RegistrationSubmissionDetails.ApplicationReferenceNumber", result);
        }

        [TestMethod]
        public void GetPanelContent_When_RegNumberSupplied_ReturnsRegistrationReferenceNumber()
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

        [TestMethod]
        public void GetPanelContent_ReturnsRegistrationReferenceNumber_EvenWhenThereIsAnAppRefNum()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                ReferenceNumber = "REG456",
                RegistrationReferenceNumber = "REG123"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("REG123", result);
        }

        [TestMethod]
        public void GetPanelContent_When_NoRegNumberSupplied_ReturnsApplicationReferenceNumber()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Pending,
                ReferenceNumber = "APP456"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            Assert.AreEqual("APP456", result);
        }

        [TestMethod]
        public void GetPanelContent_WhenStatusIsGrantedAndIsResubmission_ReturnsBothNumbers()
        {
            // Arrange
            var model = new RegistrationSubmissionDetailsViewModel
            {
                Status = RegistrationSubmissionStatus.Granted,
                IsResubmission = true,
                RegistrationReferenceNumber = "REG123",
                ReferenceNumber = "APP456"
            };

            // Act
            string? result = ReferenceNumberPanelHelper.GetPanelContent(model);

            // Assert
            string expected = new StringBuilder()
                .AppendLine("REG123")
                .AppendLine("APP456")
                .ToString();

            Assert.AreEqual(expected, result);
        }
    }
}
