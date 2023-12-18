using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

[TestClass]
public class PhaseBannerTests
{
    [TestMethod]
    public void Invoke_SetsModel()
    {
        // Arrange
        var phaseBannerOptions = new PhaseBannerOptions
        {
            ApplicationStatus = "Beta", SurveyUrl = "testUrl", Enabled = true
        };
        var options = Options.Create(phaseBannerOptions);
        var component = new PhaseBannerViewComponent(options);

        // Act
        var result = component.Invoke();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ViewViewComponentResult));
        Assert.IsNotNull(result.ViewData);
        Assert.IsNotNull(result.ViewData.Model);
        Assert.IsInstanceOfType(result.ViewData.Model, typeof(PhaseBannerModel));
        var model = (PhaseBannerModel)result.ViewData.Model;
        Assert.AreEqual(expected: "PhaseBanner.Beta", actual: model.Status);
        Assert.AreEqual(expected: phaseBannerOptions.SurveyUrl, actual: model.Url);
        Assert.AreEqual(expected: phaseBannerOptions.Enabled, actual: model.ShowBanner);
    }
}