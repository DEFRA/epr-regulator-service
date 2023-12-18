using System.Globalization;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

[TestClass]
public class LanguageSwitcherViewComponentTest
{
    [TestMethod]
    public async Task LanguageSwitcherModel_WithToken_ReturnsUrlWithToken()
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
                QueryString = new QueryString("?token=test"),
                Path = "/full-name"
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
        Assert.AreEqual(expected: "/full-name?token=test", actual: model.ReturnUrl);
    }

    [TestMethod]
    public async Task LanguageSwitcherModel_WithOutToken_ReturnsUrl()
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
                Path = "/full-name"
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
        Assert.AreEqual(expected: "/full-name", actual: model.ReturnUrl);
    }

    [TestMethod]
    public async Task LanguageSwitcherModel_WithReturnUrl_ReturnsUrl()
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
                QueryString = new QueryString("?returnUrl=test?token=testToken"),
                Path = "/cookies"
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
        Assert.AreEqual(expected: "/cookies?returnUrl=test?token=testToken", actual: model.ReturnUrl);
    }

    [TestMethod]
    public async Task LanguageSwitcherModel_WithReturnUrlAndToken_ReturnsUrl()
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
                QueryString = new QueryString("?returnUrl=test&token=test"),
                Path = "/cookies"
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
        Assert.AreEqual(expected: "/cookies?token=test&returnUrl=test", actual: model.ReturnUrl);
    }
}