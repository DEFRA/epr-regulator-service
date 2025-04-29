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
        private const string GetWasteLicenceByRegistrationMaterialId = "v{apiVersion}/registrationMaterials/{id}/wasteLicences";
        private const string UpdateRegistrationMaterialOutcome = "v{apiVersion}/registrationMaterials/{id}/outcome";
        private const string UpdateRegistrationTaskStatus = "v{apiVersion}/regulatorRegistrationTaskStatus";
        private const string UpdateApplicationTaskStatus = "v{apiVersion}/regulatorApplicationTaskStatus";

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
                    { "GetWasteLicenceByRegistrationMaterialId", GetWasteLicenceByRegistrationMaterialId },
                    { "UpdateRegistrationMaterialOutcome", UpdateRegistrationMaterialOutcome },
                    { "UpdateRegistrationTaskStatus", UpdateRegistrationTaskStatus },
                    { "UpdateApplicationTaskStatus", UpdateApplicationTaskStatus }
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
            var registrationMaterialDetail = new RegistrationMaterialDetail
            {
                Id = 123456,
                RegistrationId = 123,
                MaterialName = "Plastic",
                Status = ApplicationStatus.Granted,
            };
            return registrationMaterialDetail;
        }

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
    }
}