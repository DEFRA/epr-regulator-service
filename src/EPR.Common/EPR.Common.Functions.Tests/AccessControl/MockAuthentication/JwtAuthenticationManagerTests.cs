namespace EPR.Common.Functions.Test.AccessControl.MockAuthentication
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using EPR.Common.Functions.AccessControl.MockAuthentication;
    using EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;
    using EPR.Common.Functions.AccessControl.MockAuthentication.MockData;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class JwtAuthenticationManagerTests
    {
        private IRefreshTokenGenerator refreshTokenGenerator;
        private IJwtAuthenticationManager authenticationManager;

        // .net8 requires a longer key
        private string key = "This is my private key This is my private key This is my private key";
        private int tokenExpiryOffset = 10;

        [TestInitialize]
        public void Setup()
        {
            this.refreshTokenGenerator = new RefreshTokenGenerator();
            this.authenticationManager = new JwtAuthenticationManager(this.key, this.tokenExpiryOffset, this.refreshTokenGenerator);
        }

        [TestMethod]
        public async Task Authenticate_Should_Authenticate_User()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Authenticate_Should_Not_Authenticate_Invalid_User()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, "password2");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task Authenticate_Should_Generate_JWT_Token()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            result.JwtToken.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Authenticate_Should_Generate_Refresh_Token()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            result.RefreshToken.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Authenticate_Should_Return_JWT_Claim_Email()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result.JwtToken);
            jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "email").Value.Should().Be(user.Email);
        }

        [TestMethod]
        public async Task Authenticate_Should_Return_JWT_Claim_FirstName()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result.JwtToken);
            jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "firstName").Value.Should().Be(user.FirstName);
        }

        [TestMethod]
        public async Task Authenticate_Should_Return_JWT_Claim_Lastname()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result.JwtToken);
            jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "lastName").Value.Should().Be(user.LastName);
        }

        [TestMethod]
        public async Task Authenticate_Should_Return_JWT_Claim_Name()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result.JwtToken);
            jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "name").Value.Should().Be(user.Email);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(59)]
        public async Task Authenticate_Should_Return_Correct_Expiry_Time(int minutes)
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();
            this.authenticationManager = new JwtAuthenticationManager(this.key, minutes, this.refreshTokenGenerator);

            // Act
            var result = this.authenticationManager.Authenticate(user.Email, user.Password);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(result.JwtToken);
            var issueTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "iat").Value)).DateTime;
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "exp").Value)).DateTime;
            var span = expirationTime.Subtract(issueTime);
            span.Minutes.Should().Be(minutes);
        }

        [TestMethod]
        public async Task Authenticate_Should_Issue_Updated_Token()
        {
            // Arrange
            var user = MockUserGenerator.Generate.FirstOrDefault();
            var originalTokenInfo = this.authenticationManager.Authenticate(user.Email, user.Password);
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(originalTokenInfo.JwtToken);

            // Act
            var refreshTokenInfo = this.authenticationManager.Authenticate(user.Email, jwtSecurityToken.Claims.ToArray());

            // Assert
            refreshTokenInfo.Should().NotBeNull();
        }
    }
}