using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Cookies;
using EPR.RegulatorService.Frontend.Web.Cookies;
using EPR.RegulatorService.Frontend.Web.Extensions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Cookies;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class CookiesControllerTests
{
    ControllerContext _controllerContext;
    Mock<ICookieService> _cookieServiceMock;
    IOptions<EprCookieOptions> _eprCookieOptions;

    [TestInitialize]
    public void TestInitialize()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Cookies = Mock.Of<IRequestCookieCollection>();

        _controllerContext = new ControllerContext()
        {
            HttpContext = httpContext,
        };

        _cookieServiceMock = new Mock<ICookieService>();

        _eprCookieOptions = Options.Create(new EprCookieOptions
        {
            SessionCookieName = "sessionCookie",
            B2CCookieName = "b2cCookie",
            CookiePolicyCookieName = "policyCookie",
            AntiForgeryCookieName = "antiForgeryCookie",
            AuthenticationCookieName = "authCookie",
            TsCookieName = "tsCookie",
            TempDataCookie = "tempDataCookie"
        });
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

    [TestMethod]
    public void AcknowledgeAcceptance_LocalUrl_RedirectsToThatUrl()
    {
        // Arrange
        string localUrl = "/local/path";
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.IsLocalUrl(localUrl)).Returns(true);
        using var cookieController = new CookiesController(_cookieServiceMock.Object, _eprCookieOptions, Mock.Of<IOptions<AnalyticsOptions>>())
        {
            Url = urlHelper.Object,
            TempData = new TempDataDictionary(_controllerContext.HttpContext, Mock.Of<ITempDataProvider>()),
            ControllerContext = _controllerContext
        };

        // Act
        var result = cookieController.AcknowledgeAcceptance(localUrl);

        // Assert
        Assert.IsInstanceOfType(result, typeof(LocalRedirectResult));
        var redirectResult = (LocalRedirectResult)result;
        Assert.AreEqual(localUrl, redirectResult.Url);
    }

    private object ViewBagValue(ViewResult viewResult, string key)
    {
        return viewResult.ViewData[key];
    }
}
