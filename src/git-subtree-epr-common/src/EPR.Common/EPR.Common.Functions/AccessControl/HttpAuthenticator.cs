namespace EPR.Common.Functions.AccessControl;

using System.IdentityModel.Tokens.Jwt;
using CancellationTokens.Interfaces;
using Interfaces;
using Microsoft.Extensions.Logging;

public class HttpAuthenticator : IAuthenticator
    {
        private readonly ICancellationTokenAccessor cancellationTokenAccessor;
        private readonly ILogger<HttpAuthenticator> logger;
        private Guid? userId;
        private string? emailAddress;
        private Guid? customerOrganisationId;
        private Guid? customerId;

        public HttpAuthenticator(ILogger<HttpAuthenticator> logger, ICancellationTokenAccessor cancellationTokenAccessor)
        {
            this.logger = logger;
            this.cancellationTokenAccessor = cancellationTokenAccessor;
        }

        public Guid UserId => this.userId ?? throw new NotSupportedException("UserId cannot be used before authentication");

        public string EmailAddress => this.emailAddress ?? throw new NotSupportedException("EmailAddress cannot be used before authentication");

        public Guid CustomerOrganisationId => this.customerOrganisationId ?? throw new NotSupportedException("CustomerOrganisationId cannot be used before authentication");

        public Guid CustomerId => this.customerId ?? throw new NotSupportedException("CustomerId cannot be used before authentication");

        public async Task<bool> AuthenticateAsync(string bearerToken)
        {
            if (bearerToken is null)
            {
                return false;
            }

            var jwtEncodedString = bearerToken.Substring(7);
            var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);
            this.userId = Guid.Parse(token.Claims.First(c => c.Type == "uniqueReference").Value.ToString());
            this.emailAddress = token.Claims.First(c => c.Type == "email").Value.ToString();
            this.customerOrganisationId = Guid.Parse(token.Claims.First(c => c.Type == "customerOrganisationId").Value.ToString());
            this.customerId = Guid.Parse(token.Claims.First(c => c.Type == "customerId").Value.ToString());
            return true;
        }
    }