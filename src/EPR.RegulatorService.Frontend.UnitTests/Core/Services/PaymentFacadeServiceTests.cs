using System.Net;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

using Newtonsoft.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services;

[TestClass]
public class PaymentFacadeServiceTests
{
    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<ILogger<PaymentFacadeService>> _mockLogger;
    private Mock<ITokenAcquisition> _tokenAcquisitionMock;
    private HttpClient _httpClient;
    private IOptions<PaymentFacadeApiConfig> _paymentFacadeApiConfig;
    private PaymentFacadeService _paymentFacadeService;
    private Fixture _fixture;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockLogger = new Mock<ILogger<PaymentFacadeService>>();
        _paymentFacadeApiConfig = Options.Create(new PaymentFacadeApiConfig
        {
            BaseUrl = "http://localhost",
            Endpoints = new Dictionary<string, string>
            {
                ["SubmitOfflinePaymentPath"] = "offline-payments",
                ["GetProducerPaymentDetailsPath"] = "producer/registration-fee",
                ["GetCompliancePaymentDetailsPath"] = "compliance-scheme/registration-fee",
                ["GetProducerPaymentDetailsForResubmissionPath"] = "producer/resubmission-fee",
                ["GetCompliancePaymentDetailsResubmissionPath"] = "compliance-scheme/resubmission-fee"
            },
            DownstreamScope = "api://default"
        });
        _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
        _tokenAcquisitionMock
        .Setup(x => x.GetAccessTokenForUserAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
            It.IsAny<TokenAcquisitionOptions?>()))
        .ReturnsAsync("expectedToken");
        _httpClient = new HttpClient(_mockHandler.Object);
        _paymentFacadeService = new PaymentFacadeService(_httpClient, _tokenAcquisitionMock.Object, _paymentFacadeApiConfig, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [TestMethod]
    public async Task SubmitOfflinePaymentAsync_ReturnsSuccess_WhenResponseIsSuccessful()
    {
        // Arrange
        var request = _fixture.Create<OfflinePaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            })
            .Verifiable();

        // Act
        var result = await _paymentFacadeService.SubmitOfflinePaymentAsync(request);

        // Assert
        Assert.AreEqual(EndpointResponseStatus.Success, result);
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("offline-payments")),
            ItExpr.IsAny<CancellationToken>());
        _httpClient.DefaultRequestHeaders.Count().Should().Be(1);
        _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Bearer");
    }

    [TestMethod]
    public async Task SubmitOfflinePaymentAsync_Throws_On_Error_Response()
    {
        // Arrange
        var request = _fixture.Create<OfflinePaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            })
            .Verifiable();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            await _paymentFacadeService.SubmitOfflinePaymentAsync(request));
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_ReturnsCorrectResponse_When_SuccessStatusCode()
    {
        // Arrange
        var request = _fixture.Create<ProducerPaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new ProducerPaymentResponse()))
            })
            .Verifiable();

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request);

        // Assert
        AssertTest<ProducerPaymentResponse>(result, "producer/registration-fee");
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_ReturnsCorrectResponse_When_SuccessStatusCode()
    {
        // Arrange
        var request = _fixture.Create<CompliancePaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new CompliancePaymentResponse()))
            })
            .Verifiable();

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request);

        // Assert
        AssertTest<CompliancePaymentResponse>(result, "compliance-scheme/registration-fee");
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsForResubmissionAsync_ReturnsCorrectResponse_When_SuccessStatusCode()
    {
        // Arrange
        var request = _fixture.Create<PackagingProducerPaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new PackagingProducerPaymentResponse()))
            })
            .Verifiable();

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsForResubmissionAsync(request);

        // Assert
        AssertTest<PackagingProducerPaymentResponse>(result, "producer/resubmission-fee");
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsForResubmissionAsync_ReturnsCorrectResponse_When_SuccessStatusCode()
    {
        // Arrange
        var request = _fixture.Create<PackagingCompliancePaymentRequest>();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new PackagingCompliancePaymentResponse()))
            })
            .Verifiable();

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsForResubmissionAsync(request);

        // Assert
        AssertTest<PackagingCompliancePaymentResponse>(result, "compliance-scheme/resubmission-fee");
    }

    private void AssertTest<T>(T result, string requestUri)
    {
        Assert.IsNotNull(result);
        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
        _httpClient.DefaultRequestHeaders.Count().Should().Be(1);
        _httpClient.DefaultRequestHeaders.Authorization.Scheme.Should().Be("Bearer");
    }
}