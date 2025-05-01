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

    using FluentAssertions.Execution;

    [TestClass]
    public class ReprocessorExporterServiceTests
    {
        private const string BaseUrl = "https://example.com/";
        private const int ApiVersion = 1;
        private const string GetRegistrationByIdPath = "v{apiVersion}/registrations/{id}";
        private const string GetRegistrationMaterialByIdPath = "v{apiVersion}/registrationMaterials/{id}";
        private const string GetSiteAddressByRegistrationIdPath = "v{apiVersion}/registrations/{id}/siteAddress";
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
                    { "GetSiteAddressByRegistrationId", GetSiteAddressByRegistrationIdPath },
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
    }
}