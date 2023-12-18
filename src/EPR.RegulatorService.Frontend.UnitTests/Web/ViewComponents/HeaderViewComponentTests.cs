using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents
{

    [TestClass]
    public class HeaderViewComponentTests
    {
        private Mock<IOptions<ExternalUrlsOptions>> _options = null!;
        private HeaderViewComponent _systemUnderTest = null!;
        private const string Url = "~/home";

        [TestInitialize]
        public void Setup()
        {
            _options = new Mock<IOptions<ExternalUrlsOptions>>();
            _options.Setup(m => m.Value).Returns(new ExternalUrlsOptions { LandingPageUrl = Url });

            _systemUnderTest = new HeaderViewComponent(_options.Object);
        }

        [TestMethod]
        public void GivenInvoke_WithNoUserClaims_ThenReturnValidationResult()
        {
            // Act
            var result = _systemUnderTest.Invoke();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewViewComponentResult));
            Assert.IsNotNull(((ViewViewComponentResult)result).ViewData!["HeaderViewOverrideUrl"]);
            Assert.AreEqual(expected: Url, actual: ((ViewViewComponentResult)result!).ViewData!["HeaderViewOverrideUrl"]);
        }
    }
}