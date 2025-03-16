namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registration;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc;

[TestClass]
public class RegistrationControllerTests
{
    private RegistrationController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new RegistrationController();

    [TestMethod]
    public void UkSiteDetails_ShouldDisplayBackLink()
    {
        // Act
        var result = _controller.UkSiteDetails();

        // Assert
        result.Should().BeOfType<ViewResult>(); 

        var viewResult = (ViewResult)result;
        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.ManageRegistrations); 
    }

    [TestMethod]
    public void UkSiteDetails_Model_ShouldHaveCorrectApplicationOrganisationType()
    {
        // Act
        var result = _controller.UkSiteDetails();

        // Assert
        result.Should().BeOfType<ViewResult>(); 

        var viewResult = (ViewResult)result;
        var model = viewResult.Model as ManageRegistrationsViewModel;
        model.Should().NotBeNull(); 
        model.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor); 
    }
    [TestMethod]
    public void AuthorisedMaterials_ShouldDisplayBackLink()
    {
        // Act
        var result = _controller.AuthorisedMaterials();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;


        viewResult.ViewData["BackLinkToDisplay"].Should().Be(PagePath.ManageRegistrations);
    }
    [TestMethod]
    public void AuthorisedMaterials_Model_ShouldHaveCorrectApplicationOrganisationType()
    {
        // Act
        var result = _controller.AuthorisedMaterials();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        var model = viewResult.Model as ManageRegistrationsViewModel;
        model.Should().NotBeNull();
        model.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
    }
}
