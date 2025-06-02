using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using FluentAssertions.Execution;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services.ReprocessorExporter;

[TestClass]
public class ReprocessorExporterServiceTests
{
    private const string BaseUrl = "https://example.com/";
    private const int ApiVersion = 1;
    private const string AddMaterialQueryNote = "v{apiVersion}/regulatorApplicationTaskStatus/{id}/queryNote";
    private const string GetRegistrationByIdPath = "v{apiVersion}/registrations/{id}";
    private const string GetRegistrationMaterialByIdPath = "v{apiVersion}/registrationMaterials/{id}";
    private const string GetSiteAddressByRegistrationIdPath = "v{apiVersion}/registrations/{id}/siteAddress";
    private const string GetAuthorisedMaterialsByRegistrationId = "v{apiVersion}/registrations/{id}/authorisedMaterial";

    private const string GetWasteLicenceByRegistrationMaterialId =
        "v{apiVersion}/registrationMaterials/{id}/wasteLicences";

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
                { "GetRegistrationById", GetRegistrationByIdPath },
                { "GetRegistrationMaterialById", GetRegistrationMaterialByIdPath },
                { "GetSiteAddressByRegistrationId", GetSiteAddressByRegistrationIdPath },
                { "GetAuthorisedMaterialsByRegistrationId", GetAuthorisedMaterialsByRegistrationId },
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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", expectedRegistration.Id.ToString());

        var response = new HttpResponseMessage
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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", expectedSiteDetails.RegistrationId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSiteDetails, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", expectedRegistrationMaterial.Id.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationMaterial,
                _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedWasteLicence, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId));
    }

    [TestMethod]
    public async Task UpdateRegistrationMaterialOutcome_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateRegistrationMaterialOutcome
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());
        var request = new RegistrationMaterialOutcomeRequest
        {
            Status = ApplicationStatus.Granted, Comments = "Test comment"
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedAuthorisedMaterials,
                _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString());
        var request = new UpdateRegistrationTaskStatusRequest
        {
            TaskName = RegulatorTaskType.SiteAddressAndContactDetails.ToString(),
            RegistrationId = registrationId,
            Status = RegulatorTaskStatus.Completed.ToString()
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var request = new MarkAsDulyMadeRequest()
        {
            DeterminationDate = DateTime.Now, DulyMadeDate = DateTime.Now.AddDays(-7)
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.MarkAsDulyMadeAsync(registrationMaterialId, request));
    }

    [TestMethod]
    public async Task SubmitOfflinePayment_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        string expectedPath = SubmitOfflinePaymentPath
            .Replace("{apiVersion}", ApiVersion.ToString());

        var request = new OfflinePaymentRequest
        {
            Amount = 1234,
            PaymentReference = "Test123",
            PaymentDate = DateTime.Now.AddDays(-7),
            PaymentMethod = PaymentMethodType.Cash,
            Regulator = "Environment Agency (EA)"
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.SubmitOfflinePaymentAsync(request));
    }

    [TestMethod]
    public async Task UpdateApplicationTaskStatus_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = UpdateApplicationTaskStatus
            .Replace("{apiVersion}", ApiVersion.ToString());
        var request = new UpdateMaterialTaskStatusRequest()
        {
            TaskName = RegulatorTaskType.WasteLicensesPermitsAndExemptions.ToString(),
            RegistrationMaterialId = registrationMaterialId,
            Status = RegulatorTaskStatus.Completed.ToString()
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.UpdateRegulatorApplicationTaskStatusAsync(request));
    }

    [TestMethod]
    public async Task GetReprocessingIOByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsReprocessingIO()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var expectedReprocessingIO = CreateReprocessingIO(registrationMaterialId);
        string expectedPath = GetReprocessingIOByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedReprocessingIO, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetRegistrationByIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetSamplingPlanByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsSamplingPlan()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var expectedSamplingPlan = CreateSamplingPlan();
        string expectedPath = GetSamplingPlanByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSamplingPlan, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationPaymentFees,
                _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

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

        var expectedContent = Encoding.UTF8.GetBytes("Sample PDF content");
        const string contentType = "application/octet-stream";

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedContent)
        };
        httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

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
            .ReturnsAsync(httpResponseMessage);

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

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound); // Simulate failure

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().EndsWith("/registrations/file-download")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.DownloadSamplingInspectionFile(request));
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
            new HttpResponseMessage
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
            new HttpResponseMessage
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
            new HttpResponseMessage
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
            new HttpResponseMessage
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
            new HttpResponseMessage
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
            new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.GetRegistrationByIdWithAccreditationsAsync(registrationId, year));
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        var materialTaskId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");
        string expectedPath = AddMaterialQueryNote
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", materialTaskId.ToString());
        var request = new AddNoteRequest() { Note = "Test note", };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
            _service.AddMaterialQueryNoteAsync(materialTaskId, request));
    }

    private void SetupHttpMessageExpectations(HttpMethod method, string path,
        HttpResponseMessage responseMessage) =>
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

    private static RegistrationMaterialReprocessingIO CreateReprocessingIO(Guid registrationMaterialId) =>
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
        new RegistrationMaterialPaymentFees
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
}