
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class ManageRegistrationsReprocessorControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private RegistrationController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new RegistrationController();

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

