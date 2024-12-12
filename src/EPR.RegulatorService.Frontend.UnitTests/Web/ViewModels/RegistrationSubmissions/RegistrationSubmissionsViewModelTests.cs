namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    [TestClass]
    public class RegistrationSubmissionsViewModelTests
    {
        [TestMethod]
        public void RegistrationSubmissionsViewModel_ShouldInitializeWithNullValues()
        {
            // Arrange & Act
            var viewModel = new RegistrationSubmissionsViewModel();

            // Assert
            Assert.IsNull(viewModel.ListViewModel, "ListViewModel should be null by default.");
            Assert.IsNull(viewModel.PowerBiLogin, "PowerBiLogin should be null by default.");
            Assert.IsNull(viewModel.AgencyName, "AgencyName should be null by default.");
        }

        [TestMethod]
        public void RegistrationSubmissionsViewModel_ShouldAllowSettingListViewModel()
        {
            // Arrange
            var listViewModel = new RegistrationSubmissionsListViewModel();
            var viewModel = new RegistrationSubmissionsViewModel
            {
                // Act
                ListViewModel = listViewModel
            };

            // Assert
            Assert.AreSame(listViewModel, viewModel.ListViewModel, "ListViewModel should match the assigned instance.");
        }

        [TestMethod]
        public void RegistrationSubmissionsViewModel_ShouldAllowSettingPowerBiLogin()
        {
            // Arrange
            string powerBiLogin = "powerbi.co.uk";
            var viewModel = new RegistrationSubmissionsViewModel
            {
                // Act
                PowerBiLogin = powerBiLogin
            };

            // Assert
            Assert.AreEqual(powerBiLogin, viewModel.PowerBiLogin, "PowerBiLogin should match the assigned value.");
        }

        [TestMethod]
        public void RegistrationSubmissionsViewModel_ShouldAllowSettingAgencyName()
        {
            // Arrange
            string agencyName = "Environment Agency";
            var viewModel = new RegistrationSubmissionsViewModel
            {
                // Act
                AgencyName = agencyName
            };

            // Assert
            Assert.AreEqual(agencyName, viewModel.AgencyName, "AgencyName should match the assigned value.");
        }
    }
}
