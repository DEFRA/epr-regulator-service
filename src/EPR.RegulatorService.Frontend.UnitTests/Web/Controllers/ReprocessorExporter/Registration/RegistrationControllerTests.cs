namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registration;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc;

[TestClass]
public class RegistrationControllerTests
{
    private RegistrationsController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new RegistrationsController();

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
}
