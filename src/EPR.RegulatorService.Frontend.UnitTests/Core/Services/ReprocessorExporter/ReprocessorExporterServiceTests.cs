using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using FluentAssertions.Execution;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services.ReprocessorExporter;

using System.Globalization;

using Frontend.Core.Enums;

[TestClass]
public class ReprocessorExporterServiceTests
{
    private const string BaseUrl = "https://example.com/";
    private const int ApiVersion = 1;
    private const string AddMaterialQueryNote = "v{apiVersion}/regulatorApplicationTaskStatus/{id}/queryNote";
    private const string AddRegistrationQueryNote = "v{apiVersion}/regulatorRegistrationTaskStatus/{id}/queryNote";
    private const string GetRegistrationByIdPath = "v{apiVersion}/registrations/{id}";
    private const string GetRegistrationMaterialByIdPath = "v{apiVersion}/registrationMaterials/{id}";
    private const string GetSiteAddressByRegistrationIdPath = "v{apiVersion}/registrations/{id}/siteAddress";
    private const string GetAuthorisedMaterialsByRegistrationId = "v{apiVersion}/registrations/{id}/authorisedMaterial";
    private const string GetWasteCarrierDetailsByRegistrationId = "v{apiVersion}/registrations/{id}/wasteCarrier";
    private const string GetWasteLicenceByRegistrationMaterialId = "v{apiVersion}/registrationMaterials/{id}/wasteLicences";
    private const string UpdateRegistrationMaterialOutcome = "v{apiVersion}/registrationMaterials/{id}/outcome";
    private const string UpdateRegistrationTaskStatus = "v{apiVersion}/regulatorRegistrationTaskStatus";
    private const string UpdateApplicationTaskStatus = "v{apiVersion}/regulatorApplicationTaskStatus";

    private const string GetReprocessingIOByRegistrationMaterialIdPath =
        "v{apiVersion}/registrationMaterials/{id}/reprocessingIO";

    private const string GetSamplingPlanByRegistrationMaterialIdPath =
        "v{apiVersion}/registrationMaterials/{id}/samplingPlan";

    private const string GetPaymentFeesByRegistrationMaterialIdPath =
        "v{apiVersion}/registrationMaterials/{id}/paymentFees";

    private const string MarkAsDulyMadePath = "v{apiVersion}/registrationMaterials/{id}/markAsDulyMade";
    private const string SubmitOfflinePaymentPath = "v{apiVersion}/registrationMaterials/offlinePayment";
    private const string GetRegistrationByIdWithAccreditations = "v{apiVersion}/registrations/{id}/accreditations";
    private const string UpdateAccreditationTaskStatus = "v{apiVersion}/regulatorAccreditationTaskStatus";
    private const string GetAccreditationBusinessPlanById = "v{apiVersion}/accreditations/{id}/businessPlan";

    private const string GetSamplingPlanByAccreditationIdPath = "v{apiVersion}/accreditations/{id}/samplingPlan";
    private ReprocessorExporterService _service; // System under test
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IOptions<ReprocessorExporterFacadeApiConfig>> _optionsMock;
    private HttpClient _httpClient;
    private JsonSerializerOptions _jsonSerializerOptions;

    [TestInitialize]
    public void TestInitialize()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _optionsMock = new Mock<IOptions<ReprocessorExporterFacadeApiConfig>>();

        var tokenAcquisitionMock = new Mock<ITokenAcquisition>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        var config = new ReprocessorExporterFacadeApiConfig
        {
            BaseUrl = BaseUrl,
            ApiVersion = 1,
            DownstreamScope = "api://default",
            Endpoints = new Dictionary<string, string>
            {
                { "AddMaterialQueryNote", AddMaterialQueryNote },
                { "AddRegistrationQueryNote", AddRegistrationQueryNote },
                { "GetRegistrationById", GetRegistrationByIdPath },
                { "GetRegistrationMaterialById", GetRegistrationMaterialByIdPath },
                { "GetSiteAddressByRegistrationId", GetSiteAddressByRegistrationIdPath },
                { "GetAuthorisedMaterialsByRegistrationId", GetAuthorisedMaterialsByRegistrationId },
                { "GetWasteCarrierDetailsByRegistrationId", GetWasteCarrierDetailsByRegistrationId },
                { "GetWasteLicenceByRegistrationMaterialId", GetWasteLicenceByRegistrationMaterialId },
                { "UpdateRegistrationMaterialOutcome", UpdateRegistrationMaterialOutcome },
                { "UpdateRegistrationTaskStatus", UpdateRegistrationTaskStatus },
                { "UpdateApplicationTaskStatus", UpdateApplicationTaskStatus },
                { "GetReprocessingIOByRegistrationMaterialId", GetReprocessingIOByRegistrationMaterialIdPath },
                { "GetSamplingPlanByRegistrationMaterialId", GetSamplingPlanByRegistrationMaterialIdPath },
                { "GetPaymentFeesByRegistrationMaterialId", GetPaymentFeesByRegistrationMaterialIdPath },
                { "MarkAsDulyMade", MarkAsDulyMadePath },
                { "SubmitOfflinePayment", SubmitOfflinePaymentPath },
                { "GetRegistrationByIdWithAccreditations", GetRegistrationByIdWithAccreditations },
                { "UpdateAccreditationTaskStatus", UpdateAccreditationTaskStatus },
                { "GetSamplingPlanByAccreditationId", GetSamplingPlanByAccreditationIdPath },
                { "GetAccreditationBusinessPlanById", GetAccreditationBusinessPlanById }
            }
        };

        _optionsMock.Setup(o => o.Value).Returns(config);
        _service = new ReprocessorExporterService(_httpClient, tokenAcquisitionMock.Object, _optionsMock.Object);
    }

    [TestMethod]
    public async Task GetRegistrationByIdAsync_WhenSuccessResponse_ReturnsRegistration()
    {
        // Arrange
        var expectedRegistration = CreateRegistration();
        string expectedPath = GetRegistrationByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", expectedRegistration.Id.ToString());

        var response = () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistration, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act
        var result = await _service.GetRegistrationByIdAsync(expectedRegistration.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedRegistration);
    }

    [TestMethod]
    public async Task GetRegistrationByIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetRegistrationByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetRegistrationByIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetSiteDetailsByRegistrationIdAsync_WhenSuccessResponse_ReturnsSiteDetails()
    {
        // Arrange
        var expectedSiteDetails = CreateSiteDetails();
        string expectedPath = GetSiteAddressByRegistrationIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", expectedSiteDetails.RegistrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSiteDetails, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetSiteDetailsByRegistrationIdAsync(expectedSiteDetails.RegistrationId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSiteDetails);
        }
    }

    [TestMethod]
    public async Task GetSiteDetailsByRegistrationIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowExceptions()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetSiteAddressByRegistrationIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetSiteDetailsByRegistrationIdAsync(registrationId));
    }


    [TestMethod]
    public async Task GetRegistrationMaterialByIdAsync_WhenSuccess_ReturnsRegistrationMaterial()
    {
        // Arrange
        var expectedRegistrationMaterial = CreateRegistrationMaterial();
        string expectedPath = GetRegistrationMaterialByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", expectedRegistrationMaterial.Id.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationMaterial,
                _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetRegistrationMaterialByIdAsync(expectedRegistrationMaterial.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedRegistrationMaterial);
    }

    [TestMethod]
    public async Task GetRegistrationMaterialByIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetRegistrationMaterialByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetRegistrationMaterialByIdAsync(registrationMaterialId));
    }

    [TestMethod]
    public async Task GetWasteLicenceByRegistrationMaterialIdAsync_WhenSuccess_ReturnsRegistrationWasteLicences()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var expectedWasteLicence = CreateRegistrationWasteLicence();
        string expectedPath = GetWasteLicenceByRegistrationMaterialId
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedWasteLicence, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedWasteLicence);
    }

    [TestMethod]
    public async Task GetWasteLicenceByRegistrationMaterialId_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetWasteLicenceByRegistrationMaterialId
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId));
    }

    [TestMethod]
    public async Task GetWasteCarrierByRegistrationIdAsync_WhenSuccess_ReturnsWasteCarrierDetails()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var expectedWasteCarrier = CreateWasteCarrierDetails();
        string expectedPath = GetWasteCarrierDetailsByRegistrationId
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedWasteCarrier, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetWasteCarrierDetailsByRegistrationIdAsync(registrationMaterialId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedWasteCarrier);
    }

    [TestMethod]
    public async Task GetWasteCarrierByRegistrationMaterialId_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetWasteCarrierDetailsByRegistrationId
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetWasteCarrierDetailsByRegistrationIdAsync(registrationId));
    }


    [TestMethod]
    public async Task UpdateRegistrationMaterialOutcome_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateRegistrationMaterialOutcome
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());
        var request = new RegistrationMaterialOutcomeRequest
        {
            Status = ApplicationStatus.Granted,
            Comments = "Test comment"
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.UpdateRegistrationMaterialOutcomeAsync(registrationMaterialId, request));
    }

    [TestMethod]
    public async Task GetAuthorisedMaterialsByRegistrationIdAsync_WhenSuccess_ReturnsRegistrationAuthorisedMaterial()
    {
        // Arrange
        var registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var expectedAuthorisedMaterials = CreateRegistrationAuthorisedMaterials(registrationId);
        string expectedPath = GetAuthorisedMaterialsByRegistrationId
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedAuthorisedMaterials,
                _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetAuthorisedMaterialsByRegistrationIdAsync(registrationId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedAuthorisedMaterials);
    }

    [TestMethod]
    public async Task UpdateRegistrationTaskStatus_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateRegistrationTaskStatus
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture));
        var request = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = nameof(RegulatorTaskType.SiteAddressAndContactDetails),
            RegistrationId = registrationId,
            Status = nameof(RegulatorTaskStatus.Completed)
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.UpdateRegulatorRegistrationTaskStatusAsync(request));
    }

    [TestMethod]
    public async Task MarkAsDulyMade_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = MarkAsDulyMadePath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        var request = new MarkAsDulyMadeRequest()
        {
            DeterminationDate = DateTime.Now,
            DulyMadeDate = DateTime.Now.AddDays(-7)
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.MarkAsDulyMadeAsync(registrationMaterialId, request));
    }

    [TestMethod]
    public async Task SubmitOfflinePayment_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        string expectedPath = SubmitOfflinePaymentPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture));

        var request = new OfflinePaymentRequest
        {
            Amount = 1234,
            PaymentReference = "Test123",
            PaymentDate = DateTime.Now.AddDays(-7),
            PaymentMethod = PaymentMethodType.Cash,
            Regulator = "Environment Agency (EA)"
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.SubmitOfflinePaymentAsync(request));
    }

    [TestMethod]
    public async Task UpdateApplicationTaskStatus_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateApplicationTaskStatus
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture));
        var request = new UpdateMaterialTaskStatusRequest()
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString()
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.UpdateRegulatorApplicationTaskStatusAsync(request));
    }

    [TestMethod]
    public async Task GetReprocessingIOByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsReprocessingIO()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var expectedReprocessingInputsAndOutputs = CreateReprocessingInputsAndOutputs(registrationMaterialId);
        string expectedPath = GetReprocessingIOByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedReprocessingInputsAndOutputs, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetReprocessingIOByRegistrationMaterialIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetRegistrationByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetReprocessingIOByRegistrationMaterialIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetSamplingPlanByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsSamplingPlan()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var expectedSamplingPlan = CreateSamplingPlan();
        string expectedPath = GetSamplingPlanByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSamplingPlan, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetSamplingPlanByRegistrationMaterialIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetSamplingPlanByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetSamplingPlanByRegistrationMaterialIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetPaymentFeesByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsRegistration()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        var expectedRegistrationPaymentFees = CreateRegistrationMaterialPaymentFees();
        string expectedPath = GetPaymentFeesByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationPaymentFees,
                _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetPaymentFeesByRegistrationMaterialIdAsync(registrationMaterialId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedRegistrationPaymentFees);
    }

    [TestMethod]
    public async Task GetPaymentFeesByRegistrationMaterialIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = GetPaymentFeesByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationMaterialId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetPaymentFeesByRegistrationMaterialIdAsync(registrationMaterialId));
    }

    [TestMethod]
    public async Task DownloadSamplingAndInspectionFileAsync_WhenCalled_ReturnsHttpResponseMessage()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        const string filename = "test-document.pdf";
        var request = new FileDownloadRequest { FileId = fileId, FileName = filename };

        var expectedContent = "Sample PDF content"u8.ToArray();
        const string contentType = "application/octet-stream";

        var httpResponseMessageFactory = () =>
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            return httpResponseMessage;
        };

        _optionsMock.Object.Value.Endpoints.Add("DownloadSamplingInspectionFile",
            "v{apiVersion}/registrations/file-download");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().EndsWith("/registrations/file-download")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessageFactory);

        // Act
        var result = await _service.DownloadSamplingInspectionFile(request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBytes = await result.Content.ReadAsByteArrayAsync();
        using (new AssertionScope())
        {
            responseBytes.Should().BeEquivalentTo(expectedContent);
            result.Content.Headers.ContentType?.MediaType.Should().Be(contentType);
        }
    }

    [TestMethod]
    public async Task DownloadSamplingInspectionFile_WhenResponseNotSuccessful_ThrowsNotFoundException()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        const string filename = "test-document.pdf";
        var request = new FileDownloadRequest { FileId = fileId, FileName = filename };

        _optionsMock.Object.Value.Endpoints.Add("DownloadSamplingInspectionFile",
            "v{apiVersion}/registrations/file-download");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().EndsWith("/registrations/file-download")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.DownloadSamplingInspectionFile(request));
    }

    [TestMethod]
    public async Task DownloadSamplingAndInspectionFileAsync_WithAccreditationSubmissionType_callsAccreditationEndpoint()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        const string filename = "test-document.pdf";
        var request = new FileDownloadRequest { FileId = fileId, FileName = filename, SubmissionType = SubmissionType.Accreditation };

        var expectedContent = Encoding.UTF8.GetBytes("Accreditation Sampling & Inspection PDF file content");
        const string contentType = "application/octet-stream";

        var httpResponseMessageFactory = () =>
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expectedContent)
            };
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            return httpResponseMessage;
        };

        _optionsMock.Object.Value.Endpoints.Add("DownloadAccreditationSamplingInspectionFile", "v{apiVersion}/accreditations/file-download");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().EndsWith("/accreditations/file-download")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessageFactory);

        // Act
        var result = await _service.DownloadSamplingInspectionFile(request);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseBytes = await result.Content.ReadAsByteArrayAsync();
        using (new AssertionScope())
        {
            responseBytes.Should().BeEquivalentTo(expectedContent);
            result.Content.Headers.ContentType?.MediaType.Should().Be(contentType);
        }
    }

    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenNoYearProvided_ReturnsAllAccreditations()
    {
        var registrationId = Guid.NewGuid();

        var registration = new Registration
        {
            Id = registrationId,
            OrganisationName = "All Year Org",
            Regulator = "EA",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Materials = [
                new RegistrationMaterialSummary
                {
                    Id = Guid.NewGuid(),
                    MaterialName = "Plastic",
                    Accreditations = [
                        new Accreditation { AccreditationYear = 2023 },
                        new Accreditation { AccreditationYear = 2024 }
                    ]
                }
            ]
        };

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations",
            () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(registration, _jsonSerializerOptions))
            });

        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(registrationId);

        result.Should().NotBeNull();
        result.Materials.Single().Accreditations.Should().HaveCount(2);
    }

    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenYearProvided_ReturnsFilteredSingleAccreditation()
    {
        var registrationId = Guid.NewGuid();
        var materialId = Guid.NewGuid();
        const int year = 2025;

        var accreditation = new Accreditation
        {
            Id = Guid.NewGuid(),
            AccreditationYear = year,
            ApplicationReference = "APP-2025",
            Status = "Approved"
        };

        var registration = new Registration
        {
            Id = registrationId,
            OrganisationName = "Test Org",
            Regulator = "EA",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Materials =
            [
                new RegistrationMaterialSummary
                {
                    Id = materialId,
                    MaterialName = "Plastic",
                    Accreditations = [ accreditation ]
                }
            ]
        };

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations?year={year}",
            () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(registration, _jsonSerializerOptions))
            });

        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year);

        result.Should().NotBeNull();
        result.Materials.Should().ContainSingle();
        result.Materials.Single().Accreditations.Should().ContainSingle()
            .Which.AccreditationYear.Should().Be(year);
    }

    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenNoMaterialsHaveAccreditationsForYear_ThrowsInvalidOperation()
    {
        var registrationId = Guid.NewGuid();
        const int year = 2025;

        var registration = new Registration
        {
            Id = registrationId,
            OrganisationName = "Test Org",
            Regulator = "EA",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Materials =
            [
                new RegistrationMaterialSummary
            {
                Id = Guid.NewGuid(),
                MaterialName = "Plastic",
                Accreditations = []
            },
            new RegistrationMaterialSummary
            {
                Id = Guid.NewGuid(),
                MaterialName = "Steel",
                Accreditations = []
            }
            ]
        };

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations?year={year}",
            () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(registration, _jsonSerializerOptions))
            });

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year));
    }

    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenSomeMaterialsHaveNoMatch_StillSucceeds()
    {
        var registrationId = Guid.NewGuid();
        const int year = 2025;

        var matchingAccreditation = new Accreditation
        {
            Id = Guid.NewGuid(),
            AccreditationYear = year,
            ApplicationReference = "APP-2025",
            Status = "Approved"
        };

        var registration = new Registration
        {
            Id = registrationId,
            OrganisationName = "Partial Match Org",
            Regulator = "EA",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Materials =
            [
                new RegistrationMaterialSummary
            {
                Id = Guid.NewGuid(),
                MaterialName = "Plastic",
                Accreditations = [ matchingAccreditation ]
            },
            new RegistrationMaterialSummary
            {
                Id = Guid.NewGuid(),
                MaterialName = "Steel",
                Accreditations = [] // no match
            }
            ]
        };

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations?year={year}",
            () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(registration, _jsonSerializerOptions))
            });

        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year);

        result.Should().NotBeNull();
        result.Materials.Should().HaveCount(2);
        result.Materials.First(m => m.MaterialName == "Plastic").Accreditations.Should().ContainSingle();
        result.Materials.First(m => m.MaterialName == "Steel").Accreditations.Should().BeEmpty();
    }


    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenMultipleAccreditationsForYear_ThrowsInvalidOperation()
    {
        var registrationId = Guid.NewGuid();
        const int year = 2025;

        var registration = new Registration
        {
            Id = registrationId,
            OrganisationName = "Duplicate Year Org",
            Regulator = "EA",
            OrganisationType = ApplicationOrganisationType.Exporter,
            Materials =
            [
                new RegistrationMaterialSummary
                {
                    Id = Guid.NewGuid(),
                    MaterialName = "Plastic",
                    Accreditations =
                    [
                        new Accreditation { AccreditationYear = year },
                        new Accreditation { AccreditationYear = year }
                    ]
                }
            ]
        };

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations?year={year}",
            () => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(registration, _jsonSerializerOptions))
            });

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year));
    }

    [TestMethod]
    public async Task GetRegistrationByIdWithAccreditationsAsync_WhenInvalidGuid_ThrowsHttpRequestException()
    {
        var registrationId = Guid.NewGuid();
        const int year = 2025;

        SetupHttpMessageExpectations(HttpMethod.Get, $"v1/registrations/{registrationId}/accreditations?year={year}",
            () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year));
    }

    [TestMethod]
    public async Task GetPaymentFeesByAccreditationIdAsync_WhenResponseIsSuccess_ReturnsPaymentFees()
    {
        // Arrange
        var accreditationId = Guid.NewGuid();
        var expectedPath = $"v{ApiVersion}/accreditations/{accreditationId}/paymentFees";

        var expectedFees = new AccreditationMaterialPaymentFees
        {
            AccreditationId = accreditationId,
            OrganisationName = "Accredited Plastics Ltd",
            SiteAddress = "123 Test Lane, Recycling City",
            MaterialName = "Plastic", // Required
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            ApplicationReferenceNumber = "ACC-2025-XYZ",
            FeeAmount = 3500,
            SubmittedDate = DateTime.UtcNow.AddDays(-3),
            Regulator = "EA"
        };

        _optionsMock.Object.Value.Endpoints.Add("GetPaymentFeesByAccreditationId", "v{apiVersion}/accreditations/{id}/paymentFees");

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedFees, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetPaymentFeesByAccreditationIdAsync(accreditationId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedFees);
    }

    [TestMethod]
    public async Task GetPaymentFeesByAccreditationIdAsync_WhenNotFound_ThrowsHttpRequestException()
    {
        // Arrange
        var accreditationId = Guid.NewGuid();
        var expectedPath = $"v{ApiVersion}/accreditations/{accreditationId}/paymentFees";

        _optionsMock.Object.Value.Endpoints.Add("GetPaymentFeesByAccreditationId", "v{apiVersion}/accreditations/{id}/paymentFees");

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetPaymentFeesByAccreditationIdAsync(accreditationId));
    }

    [TestMethod]
    public async Task SubmitAccreditationOfflinePaymentAsync_WhenResponseIsSuccess_SendsCorrectRequest()
    {
        // Arrange
        var request = new AccreditationOfflinePaymentRequest
        {
            Amount = 2000,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = PaymentMethodType.BankTransfer,
            PaymentReference = "ACC-PAY-123",
            Regulator = "EA"
        };

        const string expectedPath = "v1/accreditationMaterials/offlinePayment";
        _optionsMock.Object.Value.Endpoints.Add("SubmitAccreditationOfflinePayment", expectedPath);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.SubmitAccreditationOfflinePaymentAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.ToString().Should().EndWith(expectedPath);
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("ACC-PAY-123");
    }

    [TestMethod]
    public async Task SubmitAccreditationOfflinePaymentAsync_WhenResponseIsNotSuccess_ThrowsException_AndSendsCorrectRequest()
    {
        // Arrange
        var request = new AccreditationOfflinePaymentRequest
        {
            Amount = 1500,
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            PaymentMethod = PaymentMethodType.Cheque,
            PaymentReference = "FAIL123",
            Regulator = "EA"
        };

        const string expectedPath = "v1/accreditationMaterials/offlinePayment";
        _optionsMock.Object.Value.Endpoints.Add("SubmitAccreditationOfflinePayment", expectedPath);

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.SubmitAccreditationOfflinePaymentAsync(request));

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.ToString().Should().EndWith(expectedPath);
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("FAIL123");
    }

    [TestMethod]
    public async Task MarkAccreditationAsDulyMadeAsync_WhenResponseIsSuccess_SendsCorrectRequest()
    {
        // Arrange
        var accreditationId = Guid.NewGuid();
        var request = new AccreditationMarkAsDulyMadeRequest
        {
            DeterminationDate = DateTime.UtcNow,
            DulyMadeDate = DateTime.UtcNow.AddDays(-5)
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.MarkAccreditationAsDulyMadeAsync(accreditationId, request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.ToString().Should().EndWith($"/accreditationMaterials/{accreditationId}/markAsDulyMade");
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("determinationDate").And.Contain("dulyMadeDate");
    }

    [TestMethod]
    public async Task MarkAccreditationAsDulyMadeAsync_WhenResponseIsNotSuccess_ThrowsException_AndSendsCorrectRequest()
    {
        // Arrange
        var accreditationId = Guid.NewGuid();
        var request = new AccreditationMarkAsDulyMadeRequest
        {
            DeterminationDate = DateTime.UtcNow,
            DulyMadeDate = DateTime.UtcNow.AddDays(-2)
        };

        HttpRequestMessage? capturedRequest = null;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.MarkAccreditationAsDulyMadeAsync(accreditationId, request));

        capturedRequest.Should().NotBeNull();
        capturedRequest!.Method.Should().Be(HttpMethod.Post);
        capturedRequest.RequestUri!.ToString().Should().EndWith($"/accreditationMaterials/{accreditationId}/markAsDulyMade");
        var body = await capturedRequest.Content!.ReadAsStringAsync();
        body.Should().Contain("determinationDate").And.Contain("dulyMadeDate");
    }

    [TestMethod]
    public async Task UpdateRegulatorAccreditationTaskStatusAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var accreditationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateAccreditationTaskStatus
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture));
        var request = new UpdateAccreditationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.DulyMade.ToString(),
            AccreditationId = accreditationId,
            Status = RegulatorTaskStatus.Completed.ToString()
        };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.UpdateRegulatorAccreditationTaskStatusAsync(request));
    }
    [TestMethod]
    public async Task GetSamplingPlanByAccreditationIdAsync_WhenSuccessResponse_ReturnsSamplingPlan()
    {
        // Arrange
        var accreditationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

        var expectedSamplingPlan = new AccreditationSamplingPlan
        {
            MaterialName = "Plastic",
            Files = new List<AccreditationSamplingPlanFile>
            {
                new AccreditationSamplingPlanFile
                {
                    Filename = $"FileName.pdf",
                    FileUploadType = "",
                    FileUploadStatus = "",
                    DateUploaded = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    FileId = Guid.NewGuid().ToString()
                }
            }
        };

        string expectedPath = GetSamplingPlanByAccreditationIdPath.Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
                                                                .Replace("{id}", accreditationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSamplingPlan, _jsonSerializerOptions))
        });

        // Act
        var result = await _service.GetSamplingPlanByAccreditationIdAsync(accreditationId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetSamplingPlanByAccreditationIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var accreditationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");

        string expectedPath = GetSamplingPlanByAccreditationIdPath.Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
                                                                .Replace("{id}", accreditationId.ToString());

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetSamplingPlanByAccreditationIdAsync(accreditationId));
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var materialTaskId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = AddMaterialQueryNote
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", materialTaskId.ToString());
        var request = new AddNoteRequest() { Note = "Test note", };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.AddMaterialQueryNoteAsync(materialTaskId, request));
    }


    [TestMethod]
    public async Task GetAccreditationBusinessPlanByIdAsync_ReturnsOK()
    {
        // Arrange
        var accreditationId = Guid.NewGuid();
        var expectedAccreditationBusinessPlan = CreateAccreditationBusinessPlan(accreditationId);
        string expectedPath = GetAccreditationBusinessPlanById
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", accreditationId.ToString());

        var responseFactory = () => new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedAccreditationBusinessPlan, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, responseFactory);

        // Act
        var result = await _service.GetAccreditionBusinessPlanByIdAsync(accreditationId);

        // Assert
        responseFactory.Invoke().StatusCode.Should().Be(HttpStatusCode.OK);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationTaskId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = AddRegistrationQueryNote
            .Replace("{apiVersion}", ApiVersion.ToString(CultureInfo.CurrentCulture))
            .Replace("{id}", registrationTaskId.ToString());
        var request = new AddNoteRequest { Note = "Test note", };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, () => new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.AddRegistrationQueryNoteAsync(registrationTaskId, request));
    }

    private void SetupHttpMessageExpectations(HttpMethod method, string path,
        Func<HttpResponseMessage> responseMessage) =>
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method && req.RequestUri == new Uri($"{BaseUrl}{path}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

    private static Registration CreateRegistration() =>
        new()
        {
            Id = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"),
            OrganisationName = "Blue Exports Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "Environment Agency (EA)"
        };

    private static RegistrationMaterialReprocessingIO CreateReprocessingInputsAndOutputs(Guid registrationMaterialId) =>
        new()
        {
            RegistrationId = Guid.NewGuid(),
            SiteAddress = "23, Ruby St, London, E12 3SE",
            OrganisationName = "Test Org",
            RegistrationMaterialId = registrationMaterialId,
            MaterialName = "Plastic",
            SourcesOfPackagingWaste = "N/A",
            PlantEquipmentUsed = "N/A",
            ReprocessingPackagingWasteLastYearFlag = true,
            UKPackagingWasteTonne = 100,
            NonUKPackagingWasteTonne = 50,
            NotPackingWasteTonne = 10,
            SenttoOtherSiteTonne = 5,
            ContaminantsTonne = 2,
            ProcessLossTonne = 1,
            TotalOutputs = 95,
            TotalInputs = 100
        };

    private static RegistrationMaterialDetail CreateRegistrationMaterial() =>
        new()
        {
            Id = Guid.Parse("A6B60D2B-C998-40EF-BFE4-014AE4A24624"),
            RegistrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"),
            MaterialName = "Plastic",
            Status = ApplicationStatus.Granted,
        };

    private static RegistrationAuthorisedMaterials CreateRegistrationAuthorisedMaterials(Guid registrationId) =>
        new()
        {
            RegistrationId = registrationId,
            OrganisationName = "MOCK Test Org",
            SiteAddress = "MOCK Test Address",
            MaterialsAuthorisation =
            [
                new MaterialsAuthorisedOnSite { IsMaterialRegistered = true, MaterialName = "Plastic" },
                new MaterialsAuthorisedOnSite
                {
                    IsMaterialRegistered = false,
                    MaterialName = "Steel",
                    Reason =
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce vulputate aliquet ornare. Vestibulum dolor nunc, tincidunt a diam nec, mattis venenatis sem"
                }
            ]
        };

    private static RegistrationMaterialWasteLicence CreateRegistrationWasteLicence()
    {
        var registrationMaterialWasteLicence = new RegistrationMaterialWasteLicence
        {
            RegistrationId = Guid.NewGuid(),
            OrganisationName = "Test Org",
            SiteAddress = "23, Ruby St, London, E12 3SE",
            CapacityPeriod = "Per Year",
            CapacityTonne = 50000,
            LicenceNumbers = ["DFG34573453, ABC34573453, GHI34573453"],
            MaterialName = "Plastic",
            MaximumReprocessingCapacityTonne = 10000,
            MaximumReprocessingPeriod = "Per Month",
            PermitType = "Waste Exemption",
        };

        return registrationMaterialWasteLicence;
    }

    private static RegistrationMaterialSamplingPlan CreateSamplingPlan() =>
        new()
        {
            OrganisationName = "Test Org",
            MaterialName = "Plastic",
            Files = new List<RegistrationMaterialSamplingPlanFile>
            {
                new RegistrationMaterialSamplingPlanFile
                {
                    Filename = $"FileName.pdf",
                    FileUploadType = "",
                    FileUploadStatus = "",
                    DateUploaded = DateTime.UtcNow,
                    UpdatedBy = "Test User",
                    Comments = "Test comment",
                    FileId = Guid.NewGuid().ToString()
                }
            }
        };

    private static SiteDetails CreateSiteDetails()
    {
        var expectedSiteDetails = new SiteDetails
        {
            RegistrationId = Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"),
            OrganisationName = "Test Org",
            SiteAddress = "23, Ruby St, London, E12 3SE",
            NationName = "England",
            GridReference = "SJ 854 662",
            LegalCorrespondenceAddress = "23, Ruby St, London, E12 3SE",

        };
        return expectedSiteDetails;
    }

    private static RegistrationMaterialPaymentFees CreateRegistrationMaterialPaymentFees() =>
        new()
        {
            RegistrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"),
            OrganisationName = "Test Org",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            SiteAddress = "23, Ruby St, London, E12 3SE",
            RegistrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163"),
            MaterialName = "Plastic",
            FeeAmount = 2921,
            ApplicationReferenceNumber = "ABC123456",
            SubmittedDate = DateTime.Now.AddDays(-7),
            Regulator = "GB-ENG"
        };

    private static async Task<AccreditationBusinessPlanDto> CreateAccreditationBusinessPlan(Guid id)
    {
        var queryNotes = new List<QueryNoteResponseDto>();

        var accreditationId = id;

        var queryNote1 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "First Note"
        };

        var queryNote2 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        var queryNote3 = new QueryNoteResponseDto
        {
            CreatedBy = accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        queryNotes.Add(queryNote1);
        queryNotes.Add(queryNote2);
        queryNotes.Add(queryNote3);

        var accreditationBusinessPlanDto = new AccreditationBusinessPlanDto
        {
            AccreditationId = accreditationId,
            BusinessCollectionsNotes = string.Empty,
            BusinessCollectionsPercentage = 0.00M,
            CommunicationsNotes = string.Empty,
            CommunicationsPercentage = 0.20M,
            InfrastructureNotes = "Infrastructure notes testing",
            InfrastructurePercentage = 0.30M,
            MaterialName = "Plastic",
            NewMarketsNotes = "New Market Testing notes",
            NewMarketsPercentage = 0.40M,
            NewUsersRecycledPackagingWasteNotes = string.Empty,
            NewUsersRecycledPackagingWastePercentage = 0.25M,
            NotCoveredOtherCategoriesNotes = string.Empty,
            NotCoveredOtherCategoriesPercentage = 5.00M,
            OrganisationName = "",
            RecycledWasteNotes = "No recycled waste notes at this time",
            RecycledWastePercentage = 10.00M,
            SiteAddress = "To Be Confirmed",
            TaskStatus = "Reviewed",
            QueryNotes = queryNotes
        };

        return accreditationBusinessPlanDto;
    }

    private static WasteCarrierDetails CreateWasteCarrierDetails() =>
        new()
        {
            RegistrationId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"),
            OrganisationName = "Test Org",
            SiteAddress = "23, Ruby St, London, E12 3SE",
            WasteCarrierBrokerDealerNumber = "WCB123456789",
            RegulatorRegistrationTaskStatusId = Guid.NewGuid(),
            TaskStatus = RegulatorTaskStatus.Queried
        };
}