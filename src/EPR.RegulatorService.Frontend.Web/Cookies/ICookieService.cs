namespace EPR.RegulatorService.Frontend.Web.Cookies;

public interface ICookieService
{
    void SetCookieAcceptance(bool accept, IRequestCookieCollection cookies, IResponseCookies responseCookies);

    bool HasUserAcceptedCookies(IRequestCookieCollection cookies);
}