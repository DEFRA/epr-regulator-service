using System.Net;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;

using Moq.Protected;

using Newtonsoft.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services;

[TestClass]
public class PaymentFacadeServiceTests
{
    private const string EnableRegistrationFeeCalculationViaPaymentService =
        nameof(EnableRegistrationFeeCalculationViaPaymentService);

    private Mock<HttpMessageHandler> _mockHandler;
    private Mock<ILogger<PaymentFacadeService>> _mockLogger;
    private Mock<ITokenAcquisition> _tokenAcquisitionMock;
    private Mock<IFeatureManager> _featureManagerMock;
    private HttpClient _httpClient;
    private IOptions<PaymentFacadeApiConfig> _paymentFacadeApiConfig;
    private PaymentFacadeService _paymentFacadeService;
    private Fixture _fixture;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockLogger = new Mock<ILogger<PaymentFacadeService>>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _paymentFacadeApiConfig = Options.Create(new PaymentFacadeApiConfig
        {
            BaseUrl = "http://localhost",
            Endpoints = new Dictionary<string, string>
            {
                ["SubmitOfflinePaymentPath"] = "offline-payments",
                ["GetProducerPaymentDetailsPath"] = "producer/registration-fee",
                ["GetCompliancePaymentDetailsPath"] = "compliance-scheme/registration-fee",
                ["GetProducerPaymentDetailsForResubmissionPath"] = "producer/resubmission-fee",
                ["GetCompliancePaymentDetailsResubmissionPath"] = "compliance-scheme/resubmission-fee",
                ["GetProducerPaymentDetailsBySubmissionPath"] = "producer/registration-fee/{submissionId}",
                ["GetCompliancePaymentDetailsBySubmissionPath"] = "compliance-scheme/registration-fee/{submissionId}"
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
        _featureManagerMock = new Mock<IFeatureManager>();
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(false);
        _httpClient = new HttpClient(_mockHandler.Object);
        _paymentFacadeService = new PaymentFacadeService(_httpClient, _tokenAcquisitionMock.Object, _paymentFacadeApiConfig, _featureManagerMock.Object, _mockLogger.Object);
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
    public async Task SubmitOfflinePaymentAsync_Logs_And_ReturnsFail_WhenResponseIsUnsuccessful()
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

        // Act
        var result = await _paymentFacadeService.SubmitOfflinePaymentAsync(request);

        // Assert
        Assert.AreEqual(EndpointResponseStatus.Fail, result);
        AssertTest(result, "offline-payments");       
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
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, Guid.NewGuid());

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
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, Guid.NewGuid());

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

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_WhenFlagOn_And_BySubmissionReturnsBody_ReturnsThatBody_And_DoesNotCallPost()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<ProducerPaymentRequest>();
        var submissionId = Guid.NewGuid();
        var expected = new ProducerPaymentResponse { ApplicationProcessingFee = 42m };
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"producer/registration-fee/{submissionId}", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected))
            });

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(42m);
        VerifyPost("producer/registration-fee", Times.Never());
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_WhenFlagOn_And_BySubmissionReturns404_FallsBackToPost()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<ProducerPaymentRequest>();
        var submissionId = Guid.NewGuid();
        SetupGetReturns($"producer/registration-fee/{submissionId}", HttpStatusCode.NotFound);
        SetupPostReturns("producer/registration-fee", new ProducerPaymentResponse { ApplicationProcessingFee = 99m });

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(99m);
        VerifyGet($"producer/registration-fee/{submissionId}", Times.Once());
        VerifyPost("producer/registration-fee", Times.Once());
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_WhenFlagOn_And_BySubmissionThrows_FallsBackToPost_And_LogsError()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<ProducerPaymentRequest>();
        var submissionId = Guid.NewGuid();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"producer/registration-fee/{submissionId}", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));
        SetupPostReturns("producer/registration-fee", new ProducerPaymentResponse { ApplicationProcessingFee = 7m });

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(7m);
        VerifyPost("producer/registration-fee", Times.Once());
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("By-submission fee lookup failed", StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_WhenFlagOff_CallsOnlyPost()
    {
        // Arrange (flag default in Setup is false)
        var request = _fixture.Create<ProducerPaymentRequest>();
        var submissionId = Guid.NewGuid();
        SetupPostReturns("producer/registration-fee", new ProducerPaymentResponse { ApplicationProcessingFee = 5m });

        // Act
        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        VerifyGet($"producer/registration-fee/{submissionId}", Times.Never());
        VerifyPost("producer/registration-fee", Times.Once());
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_WhenFlagOn_And_BySubmissionReturnsBody_ReturnsThatBody_And_DoesNotCallPost()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<CompliancePaymentRequest>();
        var submissionId = Guid.NewGuid();
        var expected = new CompliancePaymentResponse { ApplicationProcessingFee = 55m };
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"compliance-scheme/registration-fee/{submissionId}", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expected))
            });

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(55m);
        VerifyPost("compliance-scheme/registration-fee", Times.Never());
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_WhenFlagOn_And_BySubmissionReturns404_FallsBackToPost()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<CompliancePaymentRequest>();
        var submissionId = Guid.NewGuid();
        SetupGetReturns($"compliance-scheme/registration-fee/{submissionId}", HttpStatusCode.NotFound);
        SetupPostReturns("compliance-scheme/registration-fee", new CompliancePaymentResponse { ApplicationProcessingFee = 88m });

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(88m);
        VerifyGet($"compliance-scheme/registration-fee/{submissionId}", Times.Once());
        VerifyPost("compliance-scheme/registration-fee", Times.Once());
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_WhenFlagOn_And_BySubmissionThrows_FallsBackToPost_And_LogsError()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<CompliancePaymentRequest>();
        var submissionId = Guid.NewGuid();
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"compliance-scheme/registration-fee/{submissionId}", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));
        SetupPostReturns("compliance-scheme/registration-fee", new CompliancePaymentResponse { ApplicationProcessingFee = 3m });

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationProcessingFee.Should().Be(3m);
        VerifyPost("compliance-scheme/registration-fee", Times.Once());
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("By-submission fee lookup failed", StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_WhenFlagOff_CallsOnlyPost()
    {
        // Arrange (flag default in Setup is false)
        var request = _fixture.Create<CompliancePaymentRequest>();
        var submissionId = Guid.NewGuid();
        SetupPostReturns("compliance-scheme/registration-fee", new CompliancePaymentResponse { ApplicationProcessingFee = 6m });

        // Act
        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, submissionId);

        // Assert
        result.Should().NotBeNull();
        VerifyGet($"compliance-scheme/registration-fee/{submissionId}", Times.Never());
        VerifyPost("compliance-scheme/registration-fee", Times.Once());
    }

    [TestMethod]
    public async Task GetProducerPaymentDetailsAsync_WhenFlagOn_AppendsRequireSubmittedForApprovalTrueToGet()
    {
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<ProducerPaymentRequest>();
        var submissionId = Guid.NewGuid();
        var expected = new ProducerPaymentResponse { ApplicationProcessingFee = 1m };
        SetupGetReturnsBody($"producer/registration-fee/{submissionId}?requireSubmittedForApproval=true", expected);

        var result = await _paymentFacadeService.GetProducerPaymentDetailsAsync(request, submissionId);

        result.Should().NotBeNull();
        VerifyGet($"producer/registration-fee/{submissionId}?requireSubmittedForApproval=true", Times.Once());
    }

    [TestMethod]
    public async Task GetCompliancePaymentDetailsAsync_WhenFlagOn_AppendsRequireSubmittedForApprovalTrueToGet()
    {
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(EnableRegistrationFeeCalculationViaPaymentService))
            .ReturnsAsync(true);
        var request = _fixture.Create<CompliancePaymentRequest>();
        var submissionId = Guid.NewGuid();
        var expected = new CompliancePaymentResponse { ApplicationProcessingFee = 1m };
        SetupGetReturnsBody($"compliance-scheme/registration-fee/{submissionId}?requireSubmittedForApproval=true", expected);

        var result = await _paymentFacadeService.GetCompliancePaymentDetailsAsync(request, submissionId);

        result.Should().NotBeNull();
        VerifyGet($"compliance-scheme/registration-fee/{submissionId}?requireSubmittedForApproval=true", Times.Once());
    }

    private void SetupGetReturnsBody<T>(string uriFragment, T body) =>
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(uriFragment, StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body))
            });

    private void SetupGetReturns(string uriFragment, HttpStatusCode status) =>
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains(uriFragment, StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage { StatusCode = status });

    private void SetupPostReturns<T>(string uriFragment, T body) =>
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains(uriFragment, StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(body))
            });

    private void VerifyGet(string uriFragment, Times times) =>
        _mockHandler.Protected().Verify(
            "SendAsync",
            times,
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains(uriFragment, StringComparison.Ordinal)),
            ItExpr.IsAny<CancellationToken>());

    private void VerifyPost(string uriFragment, Times times) =>
        _mockHandler.Protected().Verify(
            "SendAsync",
            times,
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains(uriFragment, StringComparison.Ordinal)),
            ItExpr.IsAny<CancellationToken>());

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