using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Controllers.Accessibility;
using EPR.RegulatorService.Frontend.Web.ViewModels.Accessibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class AccessibilityControllerTests
    {
        private Mock<IOptions<ExternalUrlsOptions>> _urlOptionsMock = null!;
        private Mock<IOptions<EmailAddressOptions>> _emailOptionsMock = null!;
        private Mock<IOptions<SiteDateOptions>> _siteDateOptionsMock = null!;
        private AccessibilityController _systemUnderTest = default!;
        private Mock<IUrlHelper> _mockUrlHelperMock = null!;
        private readonly string _dateInput = "Jan 1, 2023";
        private readonly string _accessibilityAbilityNet = "AccessibilityAbilityNet";
        private readonly string _accessibilityContactUs = "AccessibilityContactUs";
        private readonly string _accessibilityEqualityAdvisorySupportService = "AccessibilityEqualityAdvisorySupportService";
        private readonly string _accessibilityWebContentAccessibility = "AccessibilityWebContentAccessibility";
        private readonly string _defraHelpline = "DefraHelpline";
        private DateTime _accessibilitySiteTested;
        private DateTime _accessibilityStatementPrepared;
        private DateTime _accessibilityStatementReviewed;

        [TestInitialize]
        public void Setup()
        {
            _accessibilitySiteTested = DateTime.Parse(_dateInput, System.Globalization.CultureInfo.InvariantCulture);
            _accessibilityStatementPrepared = DateTime.Parse(_dateInput, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
            _accessibilityStatementReviewed = DateTime.Parse(_dateInput, System.Globalization.CultureInfo.InvariantCulture).AddDays(2);

            _urlOptionsMock = new Mock<IOptions<ExternalUrlsOptions>>();
            _urlOptionsMock.Setup(x => x.Value).Returns(new ExternalUrlsOptions
            {
                AccessibilityAbilityNet = _accessibilityAbilityNet,
                AccessibilityContactUs = _accessibilityContactUs,
                AccessibilityEqualityAdvisorySupportService = _accessibilityEqualityAdvisorySupportService,
                AccessibilityWebContentAccessibility = _accessibilityWebContentAccessibility
            }
            );

            _emailOptionsMock = new Mock<IOptions<EmailAddressOptions>>();
            _emailOptionsMock.Setup(x => x.Value).Returns(new EmailAddressOptions
            {
                DefraHelpline = _defraHelpline
            }
            );

            _siteDateOptionsMock = new Mock<IOptions<SiteDateOptions>>();
            _siteDateOptionsMock.Setup(x => x.Value).Returns(new SiteDateOptions
            {
                AccessibilitySiteTested = _accessibilitySiteTested,
                AccessibilityStatementPrepared = _accessibilityStatementPrepared,
                AccessibilityStatementReviewed = _accessibilityStatementReviewed
            }
            );

            _mockUrlHelperMock = new Mock<IUrlHelper>();
            _systemUnderTest = new AccessibilityController(_urlOptionsMock.Object, _emailOptionsMock.Object, _siteDateOptionsMock.Object)
            {
                Url = _mockUrlHelperMock.Object
            };
        }

        [TestMethod]
        public void WhenRequestDetail_WithValidData_ThenReturnViewWithAccessibilityViewModel()
        {
            // Arrange
            const string returnUrl = "~/home";

            _mockUrlHelperMock
               .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
               .Returns(true)
               .Verifiable();
            // Act
            var result = _systemUnderTest.Detail(returnUrl);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            object? resultModel = ((ViewResult)result).Model;
            Assert.AreEqual(expected: returnUrl, actual: ((ViewResult)result).ViewData["BackLinkToDisplay"]);
            Assert.IsNotNull(resultModel);

            var resultClass = (AccessibilityViewModel)resultModel;
            Assert.IsNotNull(resultClass);
            Assert.AreEqual(expected: _accessibilityAbilityNet, actual: resultClass.AbilityNetUrl);
            Assert.AreEqual(expected: _accessibilityContactUs, actual: resultClass.ContactUsUrl);
            Assert.AreEqual(expected: _defraHelpline, actual: resultClass.DefraHelplineEmail);
            Assert.AreEqual(expected: _accessibilitySiteTested.ToString(System.Globalization.CultureInfo.InvariantCulture), actual: resultClass.SiteTestedDate);
            Assert.AreEqual(expected: _accessibilityStatementReviewed.ToString(System.Globalization.CultureInfo.InvariantCulture), actual: resultClass.StatementReviewedDate);
        }

        [TestMethod]
        public void WhenRequestDetail_WithInValidReturnUrl_ThenReturnRedirectToHomePage()
        {
            // Arrange
            const string returnUrl = "http://localhost/Index.html";

            _mockUrlHelperMock
               .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
               .Returns(false)
               .Verifiable();
            // Act
            var result = _systemUnderTest.Detail(returnUrl);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            object? resultModel = ((ViewResult)result).Model;
            Assert.IsNull(((ViewResult)result).ViewData["BackLinkToDisplay"]);

            Assert.IsNotNull(resultModel);
            var resultClass = (AccessibilityViewModel)resultModel;
            Assert.AreEqual(expected: null, actual: ((ViewResult)result).ViewData["BackLinkToDisplay"]);

            Assert.IsNotNull(resultClass);
            Assert.AreEqual(expected: _accessibilityAbilityNet, actual: resultClass.AbilityNetUrl);
            Assert.AreEqual(expected: _accessibilityContactUs, actual: resultClass.ContactUsUrl);
            Assert.AreEqual(expected: _defraHelpline, actual: resultClass.DefraHelplineEmail);
            Assert.AreEqual(expected: _accessibilitySiteTested.ToString(System.Globalization.CultureInfo.InvariantCulture), actual: resultClass.SiteTestedDate);
            Assert.AreEqual(expected: _accessibilityStatementReviewed.ToString(System.Globalization.CultureInfo.InvariantCulture), actual: resultClass.StatementReviewedDate);
        }
    }
}
