using System.Net;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services.ReprocessorExporter
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    [TestClass]
    public class ReprocessorExporterServiceTests
    {
        private const string BaseUrl = "https://example.com/";
        private const int ApiVersion = 1;
        private const string GetRegistrationByIdPath = "v{apiVersion}/registrations/{id}";
        private const string GetRegistrationMaterialByIdPath = "v{apiVersion}/registrationMaterials/{id}";
        private const string UpdateRegistrationMaterialOutcome = "v{apiVersion}/registrationMaterials/{id}/outcome";
        private const string UpdateRegistrationTaskStatus = "v{apiVersion}/regulatorRegistrationTaskStatus";
        private const string UpdateApplicationTaskStatus = "v{apiVersion}/regulatorApplicationTaskStatus";
        private const string GetSamplingPlanByRegistrationMaterialIdPath = "v{apiVersion}/registrationMaterials/{id}/samplingPlan";

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
                    { "UpdateRegistrationMaterialOutcome", UpdateRegistrationMaterialOutcome },
                    { "UpdateRegistrationTaskStatus", UpdateRegistrationTaskStatus },
                    { "UpdateApplicationTaskStatus", UpdateApplicationTaskStatus },
                    { "GetSamplingPlanByRegistrationMaterialId", GetSamplingPlanByRegistrationMaterialIdPath }
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
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRegistration.Id, result.Id);
            Assert.AreEqual(expectedRegistration.OrganisationName, result.OrganisationName);
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
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedRegistrationMaterial, _jsonSerializerOptions))
            };

            SetupHttpMessageExpectations(HttpMethod.Get, expectedPath, response);

            // Act
            var result = await _service.GetRegistrationMaterialByIdAsync(expectedRegistrationMaterial.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRegistrationMaterial.Id, result.Id);
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

        private void SetupHttpMessageExpectations(HttpMethod method, string path,
            HttpResponseMessage responseMessage) =>
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == method && req.RequestUri == new Uri($"{BaseUrl}{path}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

        private static Registration CreateRegistration()
        {
            var expectedRegistration = new Registration
            {
                Id = 123,
                OrganisationName = "Blue Exports Ltd",
                SiteAddress = "N/A",
                OrganisationType = ApplicationOrganisationType.Reprocessor,
                Regulator = "Environment Agency (EA)"
            };
            return expectedRegistration;
        }

        private static RegistrationMaterialDetail CreateRegistrationMaterial()
        {
            var expectedRegistration = new RegistrationMaterialDetail
            {
                Id = 123456,
                RegistrationId = 123,
                MaterialName = "Plastic",
                Status = ApplicationStatus.Granted,
            };
            return expectedRegistration;
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
    }
}