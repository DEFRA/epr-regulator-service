using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Cookies;
using EPR.RegulatorService.Frontend.Web.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class CookiesControllerTests
{
    ControllerContext _controllerContext;

    [TestInitialize]
    public void TestInitialize()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Cookies = Mock.Of<IRequestCookieCollection>();

        _controllerContext = new ControllerContext()
        {
            HttpContext = httpContext,
        };
    }

    [TestMethod]
    [DataRow(CookieAcceptance.Accept, true)]
    [DataRow(CookieAcceptance.Reject, false)]
    public void ShouldUpdateAcceptance(string cookies, bool acceptance)
    {
        // Arrange
        var cookieService = new Mock<ICookieService>();

        using var cookieController = new CookiesController(cookieService.Object, Mock.Of<IOptions<EprCookieOptions>>(), Mock.Of<IOptions<AnalyticsOptions>>())
        {
            TempData = new TempDataDictionary(_controllerContext.HttpContext, Mock.Of<ITempDataProvider>()),
            ControllerContext = _controllerContext
        };

        // Act
        cookieController.UpdateAcceptance("returnUrl", cookies);

        // Assert
        cookieService.Verify(cs => cs.SetCookieAcceptance(acceptance,
            _controllerContext.HttpContext.Request.Cookies,
            _controllerContext.HttpContext.Response.Cookies));
    }

    [TestMethod]
    public void ShouldRedirectToReturnUrlWhenUpdateAcceptance()
    {
        // Arrange
        const string returnUrl = "returnUrl";
        var cookieService = new Mock<ICookieService>();

        using var cookieController = new CookiesController(cookieService.Object, Mock.Of<IOptions<EprCookieOptions>>(), Mock.Of<IOptions<AnalyticsOptions>>())
        {
            TempData = new TempDataDictionary(_controllerContext.HttpContext, Mock.Of<ITempDataProvider>()),

            ControllerContext = _controllerContext
        };

        // Act
        var result = cookieController.UpdateAcceptance(returnUrl, "cookies");

        // Assert
        result.Url.Should().Be(returnUrl);
    }

    [TestMethod]

    public void ShouldRedirectToHomeWhenUrlNotLocal()
    {
        // Arrange
        const string returnUrl = "returnUrl";

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(h => h.IsLocalUrl(returnUrl)).Returns(true);

        using var cookieController = new CookiesController(Mock.Of<ICookieService>(), Mock.Of<IOptions<EprCookieOptions>>(), Mock.Of<IOptions<AnalyticsOptions>>())
        {
            Url = urlHelper.Object,
            TempData = new TempDataDictionary(_controllerContext.HttpContext, Mock.Of<ITempDataProvider>()),
            ControllerContext = _controllerContext
        };

        // Act
        var result = cookieController.AcknowledgeAcceptance(returnUrl);

        // Assert
        result.Url.Should().Be(returnUrl);
    }
}
