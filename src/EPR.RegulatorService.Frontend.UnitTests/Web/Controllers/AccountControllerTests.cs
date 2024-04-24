using EPR.RegulatorService.Frontend.Web.Controllers.Account;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        private AccountController _systemUnderTest = default!;
        private Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _microsoftIdentityOptionsMonitor = null!;
        private Mock<IUrlHelper> _mockUrlHelperMock = null!;
        private readonly string _scheme = OpenIdConnectDefaults.AuthenticationScheme;

        [TestInitialize]
        public void Setup()
        {
            var au = new MicrosoftIdentityOptions();
            _microsoftIdentityOptionsMonitor = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
            _microsoftIdentityOptionsMonitor.Setup(x => x.CurrentValue).Returns(au);
            _microsoftIdentityOptionsMonitor.Setup(x => x.Get(_scheme)).Returns(new MicrosoftIdentityOptions { ResetPasswordPolicyId = "ResetPasswordPolicyId" });

            _mockUrlHelperMock = new Mock<IUrlHelper>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Custom-Header"] = "88-test-tcb";
            httpContext.Request.Scheme = _scheme;

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,                
            };

            _systemUnderTest = new AccountController(_microsoftIdentityOptionsMonitor.Object)
            {
                Url = _mockUrlHelperMock.Object,
                ControllerContext = controllerContext,
            };
        }

        [TestMethod]
        public void WhenSignIn_WithInvalidLocalUri_ThenReturnLocalUri()
        {
            // Arrange
            string redirectUri = "http://www.google.co.uk";
            string expectedUri = "~/";

            _mockUrlHelperMock
                    .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
                   .Returns(false)
                   .Verifiable();

            _mockUrlHelperMock
                .Setup(m => m.Content(It.IsAny<string>()))
               .Returns(expectedUri)
               .Verifiable();

            // Act
            var result = _systemUnderTest.SignIn(_scheme, redirectUri);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ChallengeResult>();
            var response = result as ChallengeResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: _scheme, actual: response.AuthenticationSchemes[0]);
            Assert.IsNotNull(response.Properties);
            Assert.AreEqual(expected: expectedUri, actual: response.Properties.RedirectUri);
        }

        [TestMethod]
        public void WhenSignIn_WithValidLocalUri_ThenReturnPassedInUri()
        {
            // Arrange
            string redirectUri = "~/home";

            _mockUrlHelperMock
                    .Setup(m => m.IsLocalUrl(It.IsAny<string>()))
                   .Returns(true)
                   .Verifiable();

            _mockUrlHelperMock
                .Setup(m => m.Content(It.IsAny<string>()))
               .Returns(redirectUri)
               .Verifiable();

            // Act
            var result = _systemUnderTest.SignIn(_scheme, redirectUri);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ChallengeResult>();
            var response = result as ChallengeResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: _scheme, actual: response.AuthenticationSchemes[0]);
            Assert.IsNotNull(response.Properties);
            Assert.AreEqual(expected: redirectUri, actual: response.Properties.RedirectUri);
        }

        [TestMethod]
        public void WhenResetPassword_WithValidScheme_ThenReturnExpectedPolicyId()
        {
            // Act
            var result = _systemUnderTest.ResetPassword(_scheme);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ChallengeResult>();
            var response = result as ChallengeResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: _scheme, actual: response.AuthenticationSchemes[0]);
            Assert.IsNotNull(response.Properties);
            Assert.AreEqual(expected: "ResetPasswordPolicyId", actual: response.Properties.Items["policy"]);
        }

        [TestMethod]
        public void WhenSignOut_WithInvalidLocalUri_ThenReturnLocalUri()
        {
            // Act
            var result = _systemUnderTest.SignOut(_scheme);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<SignOutResult>();
            var response = result as SignOutResult;
            Assert.IsNotNull(response);
            Assert.AreEqual(expected: 2, actual: response.AuthenticationSchemes.Count);
            Assert.IsNotNull(response.Properties);
        }
    }
}
