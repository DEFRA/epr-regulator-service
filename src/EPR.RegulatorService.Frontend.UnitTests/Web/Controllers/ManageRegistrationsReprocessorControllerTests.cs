
using EPR.RegulatorService.Frontend.Web.Controllers.Registrations;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;

using RegistrationsController = EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration.RegistrationsController;

using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class ManageRegistrationsReprocessorControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private RegistrationsController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new RegistrationsController();

    [TestMethod]
    public void WasteLicencesScreen_ShouldRreturnViewResult()
    {
        var result = _controller.WasteLicences();

        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public void WasteLicencesScreen_ShouldDisplayBackLink()
    {
        var result = _controller.WasteLicences();

        var viewResult = (ViewResult)result;

        viewResult.ViewData.Keys.Contains(BackLinkViewDataKey).Should().BeTrue();
    }

}

