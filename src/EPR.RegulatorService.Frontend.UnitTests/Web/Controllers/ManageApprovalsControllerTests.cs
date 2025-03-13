using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class ManageApprovalsControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private ManageApprovalsController _controller;

    [TestInitialize]
    public void TestInitialize() => _controller = new ManageApprovalsController();

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