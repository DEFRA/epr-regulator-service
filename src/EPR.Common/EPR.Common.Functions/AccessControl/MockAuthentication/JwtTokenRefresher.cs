namespace EPR.Common.Functions.AccessControl.MockAuthentication;

using System.IdentityModel.Tokens.Jwt;
using Interfaces;
using Microsoft.IdentityModel.Tokens;
using Models;

public class JwtTokenRefresher : IJwtTokenRefresher
{
    private readonly byte[] tokenKey;
    private readonly IJwtAuthenticationManager authenticationManager;

    public JwtTokenRefresher(byte[] tokenKey, IJwtAuthenticationManager authenticationManager)
    {
        this.tokenKey = tokenKey;
        this.authenticationManager = authenticationManager;
    }

    public AuthenticationResponse Refresh(RefreshCredential refreshCredential)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            refreshCredential.JwtToken,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(this.tokenKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                NameClaimType = "name",
            }, out var validatedToken);
        var jwtToken = validatedToken as JwtSecurityToken;
        if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        var userName = principal.Identity.Name;
        if (refreshCredential.RefreshToken != this.authenticationManager.UsersRefreshTokens[userName])
        {
            throw new SecurityTokenException("Invalid token");
        }

        return this.authenticationManager.Authenticate(userName, principal.Claims.ToArray());
    }
}