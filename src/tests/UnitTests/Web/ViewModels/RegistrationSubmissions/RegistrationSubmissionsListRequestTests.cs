namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class RegistrationSubmissionsListRequestTests
    {
        [TestMethod]
        public void RegistrationSubmissionsListRequest_ShouldHaveDefaultPageNumber()
        {
            // Arrange & Act
            var request = new RegistrationSubmissionsListRequest();

            // Assert
            Assert.AreEqual(1, request.PageNumber, "The default PageNumber should be 1.");
        }

        [TestMethod]
        public void RegistrationSubmissionsListRequest_ShouldAllowCustomPageNumber()
        {
            // Arrange
            var request = new RegistrationSubmissionsListRequest
            {
                PageNumber = 5
            };

            // Act & Assert
            Assert.AreEqual(5, request.PageNumber, "The PageNumber should match the custom value set.");
        }

        [TestMethod]
        public void RegistrationSubmissionsListRequest_ShouldAllowNegativePageNumber()
        {
            // Arrange
            var request = new RegistrationSubmissionsListRequest
            {
                PageNumber = -1
            };

            // Act & Assert
            Assert.AreEqual(-1, request.PageNumber, "The PageNumber should match the custom negative value set.");
        }

        [TestMethod]
        public void RegistrationSubmissionsListRequest_ShouldAllowZeroPageNumber()
        {
            // Arrange
            var request = new RegistrationSubmissionsListRequest
            {
                PageNumber = 0
            };

            // Act & Assert
            Assert.AreEqual(0, request.PageNumber, "The PageNumber should match the custom value of 0 set.");
        }
    }
}
