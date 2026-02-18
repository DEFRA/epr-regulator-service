namespace EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;

using System.Security.Claims;
using Models;

public interface IJwtAuthenticationManager
{
    IDictionary<string, string> UsersRefreshTokens { get; set; }

    AuthenticationResponse Authenticate(string username, Claim[] claims);

    AuthenticationResponse Authenticate(string username, string password);
}