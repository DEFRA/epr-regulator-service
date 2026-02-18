namespace EPR.Common.Functions.AccessControl.MockAuthentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Interfaces;
using Microsoft.IdentityModel.Tokens;
using MockData;
using Models;

public class JwtAuthenticationManager : IJwtAuthenticationManager
{
    private readonly IDictionary<string, UserInformation> users;

    private readonly string key;
    private readonly int tokenExpiryOffset;
    private readonly IRefreshTokenGenerator refreshTokenGenerator;

    public JwtAuthenticationManager(string key, int tokenExpiryOffset, IRefreshTokenGenerator refreshTokenGenerator)
    {
        this.key = key;
        this.tokenExpiryOffset = tokenExpiryOffset;
        this.refreshTokenGenerator = refreshTokenGenerator;
        this.UsersRefreshTokens = new Dictionary<string, string>();

        this.users = new Dictionary<string, UserInformation>();
        var usersInformation = MockUserGenerator.Generate;
        foreach (var user in usersInformation)
        {
            this.users.Add(user.Email, user);
        }
    }

    public IDictionary<string, string> UsersRefreshTokens { get; set; }

    public AuthenticationResponse Authenticate(string username, Claim[] claims)
    {
        var key = Encoding.ASCII.GetBytes(this.key);
        var jwtSecurityToken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(this.tokenExpiryOffset),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        var refreshToken = this.refreshTokenGenerator.GenerateToken();

        if (this.UsersRefreshTokens.ContainsKey(username))
        {
            this.UsersRefreshTokens[username] = refreshToken;
        }
        else
        {
            this.UsersRefreshTokens.Add(username, refreshToken);
        }

        return new AuthenticationResponse
        {
            JwtToken = token,
            RefreshToken = refreshToken,
        };
    }

    public AuthenticationResponse Authenticate(string username, string password)
    {
        if (!this.users.Any(u => u.Key == username && u.Value.Password == password))
        {
            return null;
        }

        var user = this.users.FirstOrDefault(u => u.Key == username && u.Value.Password == password);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(this.key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("name", user.Value.Email),
                new Claim("email", user.Value.Email),
                new Claim("firstName", user.Value.FirstName),
                new Claim("lastName", user.Value.LastName),
                new Claim("uniqueReference", user.Value.UniqueReference.ToString()),
                new Claim("customerOrganisationId", user.Value.CustomerOrganisationId.ToString()),
                new Claim("customerId", user.Value.CustomerId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(this.tokenExpiryOffset),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = this.refreshTokenGenerator.GenerateToken();

        if (this.UsersRefreshTokens.ContainsKey(username))
        {
            this.UsersRefreshTokens[username] = refreshToken;
        }
        else
        {
            this.UsersRefreshTokens.Add(username, refreshToken);
        }

        return new AuthenticationResponse
        {
            JwtToken = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken,
        };
    }
}