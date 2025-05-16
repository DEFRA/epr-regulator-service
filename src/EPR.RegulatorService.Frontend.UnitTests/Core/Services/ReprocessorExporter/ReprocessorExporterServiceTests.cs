using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
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
    private const string GetRegistrationByIdPath = "v{apiVersion}/registrations/{id}";
    private const string GetRegistrationMaterialByIdPath = "v{apiVersion}/registrationMaterials/{id}";
    private const string GetSiteAddressByRegistrationIdPath = "v{apiVersion}/registrations/{id}/siteAddress";
    private const string GetAuthorisedMaterialsByRegistrationId = "v{apiVersion}/registrations/{id}/authorisedMaterial";
    private const string GetWasteLicenceByRegistrationMaterialId = "v{apiVersion}/registrationMaterials/{id}/wasteLicences";
    private const string UpdateRegistrationMaterialOutcome = "v{apiVersion}/registrationMaterials/{id}/outcome";
    private const string UpdateRegistrationTaskStatus = "v{apiVersion}/regulatorRegistrationTaskStatus";
    private const string UpdateApplicationTaskStatus = "v{apiVersion}/regulatorApplicationTaskStatus";
    private const string GetReprocessingIOByRegistrationMaterialIdPath = "v{apiVersion}/registrationMaterials/{id}/reprocessingIO";
    private const string GetSamplingPlanByRegistrationMaterialIdPath = "v{apiVersion}/registrationMaterials/{id}/samplingPlan";
    private const string GetPaymentFeesByRegistrationMaterialIdPath = "v{apiVersion}/registrationMaterials/{id}/paymentFees";
    private const string MarkAsDulyMadePath = "v{apiVersion}/registrationMaterials/{id}/markAsDulyMade";
    private const string SubmitOfflinePaymentPath = "v{apiVersion}/registrationMaterials/offlinePayment";

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
                { "SubmitOfflinePayment", SubmitOfflinePaymentPath }
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
        const int registrationId = 123;
        string expectedPath = GetRegistrationByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);
        
        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetRegistrationByIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetSiteDetailsByRegistrationIdAsync_WhenSuccessResponse_ReturnsSiteDetails()
    {
        // Arrange
        var expectedSiteDetails = CreateSiteDetails();
        string expectedPath = GetSiteAddressByRegistrationIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", expectedSiteDetails.Id.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedSiteDetails, _jsonSerializerOptions))
        };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act
         var result = await _service.GetSiteDetailsByRegistrationIdAsync(expectedSiteDetails.Id);

        // Assert
        using(new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSiteDetails);
        }
    }

    [TestMethod]
    public async Task GetSiteDetailsByRegistrationIdAsync_WhenResponseCodeIsNotSuccess_ShouldThrowExceptions()
    {
        // Arrange
        const int registrationId = 123;
        string expectedPath = GetSiteAddressByRegistrationIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetSiteDetailsByRegistrationIdAsync(registrationId));
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
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationMaterial, _jsonSerializerOptions))
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
        const int registrationMaterialId = 123;
        string expectedPath = GetRegistrationMaterialByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetRegistrationMaterialByIdAsync(registrationMaterialId));
    }
    
    [TestMethod]
    public async Task GetWasteLicenceByRegistrationMaterialIdAsync_WhenSuccess_ReturnsRegistrationWasteLicences()
    {
        // Arrange
        const int registrationMaterialId = 1234;
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
        const int registrationMaterialId = 123;
        string expectedPath = GetWasteLicenceByRegistrationMaterialId
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId));
    }
    
    [TestMethod]
    public async Task UpdateRegistrationMaterialOutcome_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        const int registrationMaterialId = 123;
        string expectedPath = UpdateRegistrationMaterialOutcome
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());
        var request = new RegistrationMaterialOutcomeRequest
        {
            Status = ApplicationStatus.Granted,
            Comments = "Test comment"
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.UpdateRegistrationMaterialOutcomeAsync(registrationMaterialId, request));
    }

    [TestMethod]
    public async Task GetAuthorisedMaterialsByRegistrationIdAsync_WhenSuccess_ReturnsRegistrationAuthorisedMaterial()
    {
        // Arrange
        const int registrationId = 1234;
        var expectedAuthorisedMaterials = CreateRegistrationAuthorisedMaterials(registrationId);
        string expectedPath = GetAuthorisedMaterialsByRegistrationId
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedAuthorisedMaterials, _jsonSerializerOptions))
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
        const int registrationId = 123;
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
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.UpdateRegulatorRegistrationTaskStatusAsync(request));
    }

    [TestMethod]
    public async Task MarkAsDulyMade_WhenResponseCodeIsNotSuccess_ShouldThrowException()
    {
        // Arrange
        const int registrationMaterialId = 123;
        string expectedPath = MarkAsDulyMadePath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var request = new MarkAsDulyMadeRequest()
        {
            DeterminationDate = DateTime.Now,
            DulyMadeDate = DateTime.Now.AddDays(-7)
        };

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

        SetupHttpMessageExpectations(HttpMethod.Post, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.MarkAsDulyMadeAsync(registrationMaterialId, request));
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
        const int registrationMaterialId = 123;
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
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.UpdateRegulatorApplicationTaskStatusAsync(request));
    }

    [TestMethod]
    public async Task GetReprocessingIOByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsReprocessingIO()
    {
        // Arrange
        int registrationMaterialId = 1;
        var expectedReprocessingIO = CreateReprocessingIO();
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
        const int registrationId = 123;
        string expectedPath = GetRegistrationByIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetRegistrationByIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetSamplingPlanByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsSamplingPlan()
    {
        // Arrange
        int registrationMaterialId = 1;
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
        const int registrationId = 123;
        string expectedPath = GetSamplingPlanByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetSamplingPlanByRegistrationMaterialIdAsync(registrationId));
    }

    [TestMethod]
    public async Task GetPaymentFeesByRegistrationMaterialIdAsync_WhenSuccessResponse_ReturnsRegistration()
    {
        // Arrange
        const int registrationMaterialId = 123;
        var expectedRegistrationPaymentFees = CreateRegistrationMaterialPaymentFees();
        string expectedPath = GetPaymentFeesByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(expectedRegistrationPaymentFees, _jsonSerializerOptions))
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
        const int registrationMaterialId = 123;
        string expectedPath = GetPaymentFeesByRegistrationMaterialIdPath
            .Replace("{apiVersion}", ApiVersion.ToString())
            .Replace("{id}", registrationMaterialId.ToString());

        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

        SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.GetPaymentFeesByRegistrationMaterialIdAsync(registrationMaterialId));
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
            Id = 123,
            OrganisationName = "Blue Exports Ltd",
            SiteAddress = "N/A",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Regulator = "Environment Agency (EA)"
        };

    private static RegistrationMaterialReprocessingIO CreateReprocessingIO() =>
        new()
        {
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
            Id = 123456,
            RegistrationId = 123,
            MaterialName = "Plastic",
            Status = ApplicationStatus.Granted,
        };

    private static RegistrationAuthorisedMaterials CreateRegistrationAuthorisedMaterials(int registrationId) =>
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
            Id = 2,
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
            RegistrationId = 123,
            OrganisationName = "Test Org",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            SiteAddress = "23, Ruby St, London, E12 3SE",
            RegistrationMaterialId = 1234,
            MaterialName = "Plastic",
            FeeAmount = 2921,
            ApplicationReferenceNumber = "ABC123456",
            SubmittedDate = DateTime.Now.AddDays(-7),
            Regulator = "GB-ENG"
        };
}