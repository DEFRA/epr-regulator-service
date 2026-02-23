namespace EPR.RegulatorService.Frontend.UnitTests.Web.FeatureManagement
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EPR.RegulatorService.Frontend.Web.FeatureManagement;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;

    [TestClass]
    public class RedirectDisabledFeatureHandlerTests
    {
        [TestMethod]
        public async Task HandleDisabledFeatures_ShouldRedirectToLandingPage()
        {
            // Arrange
            var handler = new RedirectDisabledFeatureHandler();
            var features = new List<string> { "Feature1", "Feature2" };

            var mockHttpContext = new Mock<HttpContext>();
            var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new object());

            // Act
            await handler.HandleDisabledFeatures(features, actionExecutingContext);

            // Assert
            actionExecutingContext.Result.Should().BeOfType<RedirectToActionResult>();
            var result = actionExecutingContext.Result as RedirectToActionResult;
            result.ActionName.Should().Be("LandingPage");
            result.ControllerName.Should().Be("home");
        }
    }
}
