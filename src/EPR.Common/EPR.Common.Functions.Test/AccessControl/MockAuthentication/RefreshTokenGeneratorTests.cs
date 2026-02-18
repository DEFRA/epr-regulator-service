namespace EPR.Common.Functions.Test.AccessControl.MockAuthentication
{
    using EPR.Common.Functions.AccessControl.MockAuthentication;
    using EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RefreshTokenGeneratorTests
    {
        private IRefreshTokenGenerator refreshTokenGenerator;

        [TestInitialize]
        public void Setup()
        {
            this.refreshTokenGenerator = new RefreshTokenGenerator();
        }

        [TestMethod]
        public async Task GenerateToken_Generates_Token()
        {
            // Act
            var token = this.refreshTokenGenerator.GenerateToken();

            // Assert
            token.Should().NotBeNull();
        }
    }
}