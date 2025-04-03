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
    [TestClass]
    public class ReprocessorExporterServiceTests
    {
        private const string BaseUrl = "https://example.com";
        private const string GetRegistrationByIdPath = "/registrations/{id}";
        private const string GetRegistrationMaterialByIdPath = "/registrationMaterials/{id}";
        private const string UpdateRegistrationMaterialOutcome = "/registrationMaterials/{id}/outcome";

        private ReprocessorExporterService _service; // System under test

        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IOptions<ReprocessorExporterFacadeApiConfig>> _optionsMock;
        private HttpClient _httpClient;
        
        [TestInitialize]
        public void TestInitialize()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _optionsMock = new Mock<IOptions<ReprocessorExporterFacadeApiConfig>>();

            var tokenAcquisitionMock = new Mock<ITokenAcquisition>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            var config = new ReprocessorExporterFacadeApiConfig
            {
                BaseUrl = BaseUrl,
                DownstreamScope = "api://default",
                Endpoints = new Dictionary<string, string>
                {
                    { "GetRegistrationById", GetRegistrationByIdPath },
                    { "GetRegistrationMaterialById", GetRegistrationMaterialByIdPath },
                    { "UpdateRegistrationMaterialOutcome", UpdateRegistrationMaterialOutcome }
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
            string expectedPath = GetRegistrationByIdPath.Replace("{id}", expectedRegistration.Id.ToString());

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedRegistration))
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
            string expectedPath = GetRegistrationByIdPath.Replace("{id}", registrationId.ToString());

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
            string expectedPath = GetRegistrationMaterialByIdPath.Replace("{id}", expectedRegistrationMaterial.Id.ToString());

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(expectedRegistrationMaterial))
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
            string expectedPath = GetRegistrationMaterialByIdPath.Replace("{id}", registrationMaterialId.ToString());

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
            string expectedPath = UpdateRegistrationMaterialOutcome.Replace("{id}", registrationMaterialId.ToString());
            var request = new RegistrationMaterialOutcomeRequest
            {
                Status = ApplicationStatus.Granted,
                Comments = "Test comment"
            };

            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError };

            SetupHttpMessageExpectations(HttpMethod.Patch, expectedPath, response);

            // Act/Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => _service.UpdateRegistrationMaterialOutcomeAsync(registrationMaterialId, request));
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
    }
}