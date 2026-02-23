using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Cookies;

[TestClass]
public class CookieServiceTests
{
    private const string CookieName = ".epr_cookies_policy";
    private const string GoogleAnalyticsDefaultCookieName = "_ga";
    private CookieService _systemUnderTest = null!;
    private Mock<IOptions<EprCookieOptions>> _cookieOptions = null!;
    private Mock<IOptions<AnalyticsOptions>> _googleAnalyticsOptions = null!;
    private Mock<ILogger<CookieService>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _cookieOptions = new Mock<IOptions<EprCookieOptions>>();
        _loggerMock = new Mock<ILogger<CookieService>>();
    }

    [TestMethod]
    public void SetCookieAcceptance_ShouldThrowArgumentNullException_WhenCookiePolicyCookieNameNotSet()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        HttpContext context = new DefaultHttpContext();
        MockService();

        // Act
        var act = () => _systemUnderTest.SetCookieAcceptance(true, requestCookieCollection, context.Response.Cookies);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void SetCookieAcceptance_True_ReturnValidCookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection();
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(true, requestCookieCollection, context.Response.Cookies);

        // Assert
        string? cookieValue = GetCookieValueFromResponse(context.Response, CookieName);
        cookieValue.Should().Be("True");
    }

    [TestMethod]
    public void SetCookieAcceptance_False_ReturnValidCookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection();
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(false, requestCookieCollection, context.Response.Cookies);

        // Assert
        string? cookieValue = GetCookieValueFromResponse(context.Response, CookieName);
        cookieValue.Should().Be("False");
    }

    [TestMethod]
    public void SetCookieAcceptance_False_ResetsGACookie()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(GoogleAnalyticsDefaultCookieName, "1234");
        var context = new DefaultHttpContext();
        MockService(CookieName);

        // Act
        _systemUnderTest.SetCookieAcceptance(false, requestCookieCollection, context.Response.Cookies);

        // Assert
        string? cookieValue = GetCookieValueFromResponse(context.Response, GoogleAnalyticsDefaultCookieName);
        cookieValue.Should().Be("1234");
    }

    [TestMethod]
    public void HasUserAcceptedCookies_ShouldThrowArgumentNullException_WhenCookiePolicyCookieNameNotSet()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        MockService();

        // Act
        var act = () => _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void HasUserAcceptedCookies_True_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(CookieName, "True");
        MockService(CookieName);

        // Act
        bool result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(true);
    }

    [TestMethod]
    public void HasUserAcceptedCookies_False_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection(CookieName, "False");
        MockService(CookieName);

        // Act
        bool result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(false);
    }

    [TestMethod]
    public void HasUserAcceptedCookies_NoCookie_ReturnsValidValue()
    {
        // Arrange
        var requestCookieCollection = MockRequestCookieCollection("test", "test");
        MockService(CookieName);

        // Act
        bool result = _systemUnderTest.HasUserAcceptedCookies(requestCookieCollection);

        // Assert
        result.Should().Be(false);
    }

    private static IRequestCookieCollection MockRequestCookieCollection(string key = "", string value = "")
    {
        var requestFeature = new HttpRequestFeature();
        var featureCollection = new FeatureCollection();
        requestFeature.Headers = new HeaderDictionary();
        if (key != string.Empty && value != string.Empty)
        {
            requestFeature.Headers.Append(HeaderNames.Cookie, new StringValues(key + "=" + value));
        }

        featureCollection.Set<IHttpRequestFeature>(requestFeature);
        var cookiesFeature = new RequestCookiesFeature(featureCollection);
        return cookiesFeature.Cookies;
    }

    private static string? GetCookieValueFromResponse(HttpResponse response, string cookieName)
    {
        foreach (var headers in response.Headers)
        {
            if (headers.Key != "Set-Cookie")
            {
                continue;
            }

            string header = headers.Value!;
            if (!header.StartsWith($"{cookieName}=", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            int p1 = header.IndexOf('=');
            int p2 = header.IndexOf(';');
            return header.Substring(p1 + 1, p2 - p1 - 1);
        }

        return null;
    }

    private void MockService(string? cookieName = null)
    {
        var eprCookieOptions = new EprCookieOptions();
        if(cookieName != null)
        {
            eprCookieOptions.CookiePolicyCookieName = cookieName;   
        }

        var googleAnalyticsOptions = new AnalyticsOptions { CookiePrefix = GoogleAnalyticsDefaultCookieName };

        _cookieOptions = new Mock<IOptions<EprCookieOptions>>();
        _cookieOptions.Setup(ap => ap.Value).Returns(eprCookieOptions);

        _googleAnalyticsOptions = new Mock<IOptions<AnalyticsOptions>>();
        _googleAnalyticsOptions.Setup(ap => ap.Value).Returns(googleAnalyticsOptions);

        _systemUnderTest = new CookieService(
            _loggerMock.Object,
            _cookieOptions.Object,
            _googleAnalyticsOptions.Object);
    }
}