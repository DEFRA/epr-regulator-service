namespace EPR.Common.Functions.Test.AccessControl.MockAuthentication
{
    using System.Text;
    using EPR.Common.Functions.AccessControl.MockAuthentication;
    using EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;
    using EPR.Common.Functions.AccessControl.MockAuthentication.MockData;
    using EPR.Common.Functions.AccessControl.MockAuthentication.Models;
    using FluentAssertions;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class JwtTokenRefresherTests
    {
        private IRefreshTokenGenerator refreshTokenGenerator;
        private IJwtAuthenticationManager authenticationManager;
        private IJwtTokenRefresher tokenRefresher;

        // .net8 requires a longer key
        private string key = "This is my private key This is my private key This is my private key";
        private int tokenExpiryOffset = 10;

        [TestInitialize]
        public void Setup()
        {
            this.refreshTokenGenerator = new RefreshTokenGenerator();
            this.authenticationManager = new JwtAuthenticationManager(this.key, this.tokenExpiryOffset, this.refreshTokenGenerator);
            this.tokenRefresher = new JwtTokenRefresher(Encoding.ASCII.GetBytes(this.key), this.authenticationManager);
        }

        [TestMethod]
        public async Task JwtTokenRefresher_Should_Refresh_Token()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();
            var originalTokenInfo = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Act
            var refreshTokenInfo = this.tokenRefresher.Refresh(new RefreshCredential()
            {
                JwtToken = originalTokenInfo.JwtToken,
                RefreshToken = originalTokenInfo.RefreshToken,
            });

            // Assert
            refreshTokenInfo.Should().NotBeNull();
        }

        [TestMethod]
        public async Task JwtTokenRefresher_Should_Not_Refresh_Token_With_Invalid_RefreshToken()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();
            var originalTokenInfo = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            Assert.ThrowsException<SecurityTokenException>(() => this.tokenRefresher.Refresh(new RefreshCredential()
            {
                JwtToken = originalTokenInfo.JwtToken,
                RefreshToken = "Invalid Refresh Token",
            }));
        }
    }
}