namespace EPR.Common.Functions.AccessControl.MockAuthentication.Models;

public class RefreshCredential
{
    public string JwtToken { get; set; }

    public string RefreshToken { get; set; }
}