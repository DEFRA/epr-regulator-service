using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using System.Globalization;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class LanguageSwitcherViewComponentTest
{
    private const string TestQueryString = "?token=test&abc=xyz";
    private const string TestPath = "/test";

    [TestMethod]
    public async Task LanguageSwitcherModel_WithQueryString_ReturnsUrlWithQueryString()
    {
        // Arrange
        var mockLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
        mockLocalizationOptions.Setup(o => o.Value).Returns(new RequestLocalizationOptions
        {
            SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB"), new CultureInfo("cy-GB") }
        });

        var mockFeatureManager = new Mock<IFeatureManager>();
        mockFeatureManager.Setup(f => f.IsEnabledAsync(FeatureFlags.ShowLanguageSwitcher)).Returns(Task.FromResult(true));

        var viewComponent = new LanguageSwitcherViewComponent(mockLocalizationOptions.Object, mockFeatureManager.Object);

        var mockHttpContext = new DefaultHttpContext
        {
            Request =
            {
                QueryString = new QueryString(TestQueryString),
                Path = TestPath
            }
        };

        var mockRequestCultureFeature = new Mock<IRequestCultureFeature>();
        mockRequestCultureFeature.Setup(m => m.RequestCulture).Returns(new RequestCulture("en-GB"));

        mockHttpContext.Features.Set(mockRequestCultureFeature.Object);

        viewComponent.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = mockHttpContext
            }
        };

        // Act
        var result = await viewComponent.InvokeAsync();

        // Assert
        var viewResult = result as ViewViewComponentResult;
        var model = viewResult?.ViewData?.Model as LanguageSwitcherModel;

        Assert.IsNotNull(model);
        Assert.AreEqual(expected: $"{TestPath}{TestQueryString}", actual: model.ReturnUrl);
    }

    [TestMethod]
    public async Task LanguageSwitcherModel_WithoutQueryString_ReturnsUrl()
    {
        // Arrange
        var mockLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
        mockLocalizationOptions.Setup(o => o.Value).Returns(new RequestLocalizationOptions
        {
            SupportedCultures = new List<CultureInfo> { new CultureInfo("en-GB"), new CultureInfo("cy-GB") }
        });

        var mockFeatureManager = new Mock<IFeatureManager>();
        mockFeatureManager.Setup(f => f.IsEnabledAsync(FeatureFlags.ShowLanguageSwitcher)).Returns(Task.FromResult(true));

        var viewComponent = new LanguageSwitcherViewComponent(mockLocalizationOptions.Object, mockFeatureManager.Object);

        var mockHttpContext = new DefaultHttpContext
        {
            Request =
            {
                QueryString = new QueryString(),
                Path = TestPath
            }
        };

        var mockRequestCultureFeature = new Mock<IRequestCultureFeature>();
        mockRequestCultureFeature.Setup(m => m.RequestCulture).Returns(new RequestCulture("en-GB"));

        mockHttpContext.Features.Set(mockRequestCultureFeature.Object);

        viewComponent.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new ViewContext
            {
                HttpContext = mockHttpContext
            }
        };

        // Act
        var result = await viewComponent.InvokeAsync();

        // Assert
        var viewResult = result as ViewViewComponentResult;
        var model = viewResult?.ViewData?.Model as LanguageSwitcherModel;

        Assert.IsNotNull(model);
        Assert.AreEqual(expected: TestPath, actual: model.ReturnUrl);
    }
}