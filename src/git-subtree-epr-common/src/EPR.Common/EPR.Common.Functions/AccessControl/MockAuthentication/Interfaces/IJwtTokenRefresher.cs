namespace EPR.Common.Functions.AccessControl.MockAuthentication.Interfaces;

using Models;

public interface IJwtTokenRefresher
{
    AuthenticationResponse Refresh(RefreshCredential refreshCredential);
}