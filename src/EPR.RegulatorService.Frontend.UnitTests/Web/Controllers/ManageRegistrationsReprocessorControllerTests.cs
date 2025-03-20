
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Registrations;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

using RegistrationsController = EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations.RegistrationsController;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class ManageRegistrationsReprocessorControllerTests : RegistrationTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private RegistrationsController _controller;

    [TestInitialize]
    public void TestInitialize()
    {
        SetupBase();

        _controller = new RegistrationsController(
            _sessionManagerMock.Object,
            _configurationMock.Object);
    }

    [TestMethod]
    public void WasteLicencesScreen_ShouldReturnViewResult()
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

