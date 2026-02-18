namespace EPR.Common.Functions.Controllers;

using AccessControl.MockAuthentication.Interfaces;
using AccessControl.MockAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IJwtAuthenticationManager jwtAuthenticationManager;
    private readonly IJwtTokenRefresher tokenRefresher;
    private readonly IConfiguration configuration;

    public AuthController(IJwtAuthenticationManager jwtAuthenticationManager, IJwtTokenRefresher tokenRefresher, IConfiguration configuration)
    {
        this.jwtAuthenticationManager = jwtAuthenticationManager;
        this.tokenRefresher = tokenRefresher;
        this.configuration = configuration;
    }

    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] UserCredentials userCredentials)
    {
        var token = this.jwtAuthenticationManager.Authenticate(userCredentials.Username, userCredentials.Password);
        if (token == null)
        {
            return this.Unauthorized();
        }

        return this.Ok(token);
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshCredential refreshCredential)
    {
        var token = this.tokenRefresher.Refresh(refreshCredential);
        if (token == null)
        {
            return this.Unauthorized();
        }

        return this.Ok(token);
    }
}