namespace EPR.Common.Functions.AccessControl.MockAuthentication;

using System.Security.Cryptography;
using Interfaces;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateToken()
    {
        var randomNumber = new byte[32];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}