using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Security.Claims;
using EPR.Common.Authorization.Models;
using EPR.RegulatorService.Frontend.Core.Services;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewComponents;

public abstract class ViewComponentsTestBase
{
    protected readonly Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService> _authorizationServiceMock = new();
    protected readonly Mock<IFacadeService> _facadeServiceMock = new();
    protected readonly Mock<ISessionManager<JourneySession>> _sessionManager = new();
    protected readonly Mock<IHttpContextAccessor> _viewComponentHttpContextAccessor = new();
    protected readonly Mock<ISession> _session = new();
    protected readonly Mock<HttpContext> _httpContextMock = new();
    protected readonly Mock<ClaimsPrincipal> _userMock = new();
    protected readonly ViewContext _viewContext = new();
    protected readonly ViewComponentContext _viewComponentContext = new();
    
    protected JourneySession JourneySessionMock { get; set; }

    protected void SetViewComponentContext(string requestPath, ViewComponent component, UserData? userData)
    {
        var claims = new List<Claim>();
        if (userData != null)
        {
            claims.Add(new(ClaimTypes.UserData, Newtonsoft.Json.JsonConvert.SerializeObject(userData)));
        }
        
        _userMock.Setup(x => x.Claims).Returns(claims);
        _httpContextMock.Setup(x => x.Request.Path).Returns($"/{requestPath}");
        _httpContextMock.Setup(x => x.Session).Returns(_session.Object);
        _httpContextMock.Setup(x => x.User).Returns(_userMock.Object);
        _viewComponentHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);
        _viewContext.HttpContext = _httpContextMock.Object;
        _viewComponentContext.ViewContext = _viewContext;
        component.ViewComponentContext = _viewComponentContext;
    }
}