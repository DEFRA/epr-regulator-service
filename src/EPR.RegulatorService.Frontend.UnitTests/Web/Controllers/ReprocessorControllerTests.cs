using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Reprocessor;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration.Reprocessor;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Reprocessor;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class ReprocessorControllerTests
{
    
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private RegistrationController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new RegistrationController();

    [TestMethod]
    public void AuthorisedMaterials_ShouldDisplayBackLink()
    {
        // Act
        var result = _controller.AuthorisedMaterials();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;


        viewResult.ViewData[BackLinkViewDataKey].Should().Be(PagePath.ManageApplications);
    }
    [TestMethod]
    public void UkSiteDetails_ShouldDisplayBackLink()
    {
        // Act
        var result = _controller.UkSiteDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.ViewData[BackLinkViewDataKey].Should().Be(PagePath.ManageApplications);
    }
}