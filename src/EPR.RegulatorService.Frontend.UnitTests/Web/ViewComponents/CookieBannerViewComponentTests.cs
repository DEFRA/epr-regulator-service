namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using EPR.RegulatorService.Frontend.Web.Constants;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using EPR.RegulatorService.Frontend.Web.ViewModels.Cookies;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Routing;

[TestClass]
public class CookieBannerViewComponentTests : ViewComponentsTestBase
{
    Fixture _fixture;

    [TestInitialize]
    public void TestInitialize()
    {
        _fixture = new();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [TestMethod]
    [DataRow(true, false, null, false, "", null, "Should show banner, no acknowledgement, no analytics")]
    [DataRow(false, false, CookieAcceptance.Accept, true, "Cookies", "consentCookie", "No banner, accept analytics, no acknowledgement")]
    [DataRow(false, true, CookieAcceptance.CookieAcknowledgement, false, "", null, "Should show acknowledgement, no banner, no analytics")]
    public void ShouldRetrieveCookieBanner(bool showBanner, bool showAcknowledgement, string? cookieAcknowledgement, bool acceptAnalytics, string showBannerString, string? consentCookie, string because)
    {
        // Arrange
        const string currentPage = "/currentPage";

        var eprCookieOptions = _fixture.Create<EprCookieOptions>();
        var options = new Mock<IOptions<EprCookieOptions>>();
        options.Setup(o => o.Value).Returns(eprCookieOptions);

        var cookies = new Mock<IRequestCookieCollection>();

        cookies.Setup(c => c[eprCookieOptions.CookiePolicyCookieName]).Returns(consentCookie);

        _fixture.Customize<HttpRequest>(
            c => c.With(req => req.Path, new PathString(currentPage))
            .With(req => req.Cookies, cookies.Object));

        var httpContext = _fixture.Create<DefaultHttpContext>();


        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        tempData[CookieAcceptance.CookieAcknowledgement] = cookieAcknowledgement;

        var routeData = _fixture.Create<RouteData>();
        routeData.Values["controller"] = showBannerString;

        _httpContextMock.Setup(ctx => ctx.Request.Cookies).Returns(cookies.Object);
        _viewContext.RouteData = routeData;
        _viewContext.TempData = tempData;

        var component = new CookieBannerViewComponent(options.Object);

        SetViewComponentContext(currentPage, component, null);

        // Act
        var result = component.Invoke();

        // Assert
        result.Should().NotBeNull();
        var viewResult = result as ViewViewComponentResult;
        var model = viewResult?.ViewData?.Model as CookieBannerModel;

        model.Should().NotBeNull();
        model!.CurrentPage.Should().Be($"/{currentPage}", because);
        model.ShowBanner.Should().Be(showBanner, because);
        model.ShowAcknowledgement.Should().Be(showAcknowledgement, because);
        model.AcceptAnalytics.Should().Be(acceptAnalytics, because);
    }
}

