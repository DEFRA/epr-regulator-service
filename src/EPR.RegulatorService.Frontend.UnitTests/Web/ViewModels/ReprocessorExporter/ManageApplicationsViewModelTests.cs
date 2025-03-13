using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.ReprocessorExporter;

[TestClass]
public class ManageApplicationsViewModelTests
{
    [DataTestMethod]
    [DataRow(ApplicationType.Registration, "Manage Registrations")]
    [DataRow(ApplicationType.Accreditation, "Manage Accreditations")]
    public void Title_ShouldReturnExpectedValue(ApplicationType applicationType, string expectedTitle)
    {
        // Act
        var viewModel = new ManageApplicationsViewModel
        {
            ApplicationType = applicationType
        };  

        // Assert
        viewModel.Title.Should().Be(expectedTitle);
    }
}