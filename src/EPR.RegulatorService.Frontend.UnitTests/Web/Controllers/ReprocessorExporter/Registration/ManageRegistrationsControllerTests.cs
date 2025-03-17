using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registration;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registration;

[TestClass]
public class ManageRegistrationsControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private ManageRegistrationsController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new ManageRegistrationsController();

    [TestMethod]
    public void Index_ShouldDisplayBackLink()
    {
        // Act
        var result = _controller.Index();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        viewResult.ViewData.Keys.Contains(BackLinkViewDataKey).Should().BeTrue();
    }
}