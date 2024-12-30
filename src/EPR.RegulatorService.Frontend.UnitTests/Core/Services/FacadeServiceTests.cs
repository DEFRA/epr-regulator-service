using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services
{
    [TestClass]
    public class FacadeServiceTests
    {
        private const string TestOrgName = "Test org name";
        private const string ApprovedPerson = "ApprovedPerson";
        private const string DelegatedPerson = "DelegatedPerson";
        private Mock<HttpMessageHandler> _mockHandler;
        private Mock<ILogger<FacadeService>> _mockLogger;
        private Mock<ITokenAcquisition> _tokenAcquisitionMock;
        private HttpClient _httpClient;
        private IOptions<PaginationConfig> _paginationConfig;
        private IOptions<FacadeApiConfig> _facadeApiConfig;
        private FacadeService _facadeService;
        private Fixture _fixture;

        private readonly Guid _organisationId = Guid.NewGuid();
        private readonly Guid _connExternalId = Guid.NewGuid();
        private readonly Guid _promotedPersonExternalId = Guid.NewGuid();

        private const int PAGE_SIZE = 10;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _mockLogger = new Mock<ILogger<FacadeService>>();
            _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            _httpClient = new HttpClient(_mockHandler.Object) { BaseAddress = new Uri("http://localhost") };
            _paginationConfig = Options.Create(new PaginationConfig { PageSize = PAGE_SIZE });
            _facadeApiConfig = Options.Create(new FacadeApiConfig
            {
                Endpoints = new Dictionary<string, string>
                {
                    ["PendingApplications"] = "http://testurl.com",
                    ["UpdateEnrolment"] = "http://testurl.com",
                    ["OrganisationEnrolmentsPath"] = "organisations/{0}/pending-applications",
                    ["OrganisationTransferNationPath"] = "accounts/transfer-nation",
                    ["SendEnrolmentUpdatedEmails"] = "regulators/accounts/govNotification",
                    ["EnrolInvitedUser"] = "accounts-management/enrol-invited-user",
                    ["UserAccounts"] = "user-accounts",
                    ["OrganisationDetails"] = "organisations/organisation-details?externalId={0}",
                    ["GetOrganisationUsersByOrganisationExternalIdPath"] = "organisations/users-by-organisation-external-id?externalId={0}",
                    ["PomSubmissions"] = "http://testurl.com/PomSubmissions",
                    ["RegistrationSubmissions"] = "http://testurl.com/RegistrationSubmissions",
                    ["OrganisationsSearchPath"] = "organisations/search-organisations?currentPage={0}&pageSize={1}&searchTerm={2}",
                    ["PomSubmissionDecision"] = "http://testurl.com",
                    ["OrganisationsRemoveApprovedUser"] = "organisations/remove-approved-users?connExternalId={0}&organisationId={1}&promotedPersonExternalId={2}",
                    ["AddRemoveApprovedUser"] = "/accounts-management/add-remove-approved-users",
                    ["RegistrationSubmissionDecisionPath"] = "http://testurl.com",
                    ["FileDownload"] = "https://api.example.com/file/download",
                    ["OrganisationRegistrationSubmissions"] = "registrations/get-organisations&currentPage={0}&pageSize={1}",
                    ["OrganisationRegistrationSubmissionDecisionPath"] = "organisation-registration-submission-decision",
                    ["GetOrganisationRegistrationSubmissionDetailsPath"] = "registrations-submission-details/submissionId/{0}",
                    ["GetOrganisationRegistrationSubmissionsPath"] = "organisation-registration-submissions",
                    ["SubmitRegistrationFeePaymentPath"] = "organisation-registration-fee-payment"
                },
                DownstreamScope = "api://default"
            });

            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig, _mockLogger.Object);
            _fixture = new Fixture();
        }

        [TestMethod]
        public async Task GetTestMessageAsync_ShouldReturnTestMessage_WhenStatusCodeIsOk()
        {
            // Arrange
            string testMessage = "Test Message";

            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(testMessage) };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            string result = await _facadeService.GetTestMessageAsync();

            // Assert
            result.Should().Be(testMessage);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetUserApplicationsByOrganisation_ShouldReturnPaginatedList_WhenRequestIsCorrect()
        {
            // Arrange
            var testOrgAppList = _fixture.Create<PaginatedList<OrganisationApplications>>();
            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.GetUserApplicationsByOrganisation(null, null, 1);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetUserApplicationsByOrganisation_ShouldThrowException_WhenStatusCodeIsError()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            Func<Task> act = async () => await _facadeService.GetUserApplicationsByOrganisation(null, null, 1);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetUserApplicationsByOrganisation_ShouldThrow_WhenContentIsNull()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = null };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            Func<Task> act = async () => await _facadeService.GetUserApplicationsByOrganisation(null, null, 1);

            // Assert
            await act.Should().ThrowAsync<JsonException>();

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        [DataRow(null, TestOrgName)]
        [DataRow(ApprovedPerson, TestOrgName)]
        [DataRow(ApprovedPerson, null)]
        [DataRow(DelegatedPerson, TestOrgName)]
        [DataRow(DelegatedPerson, null)]
        public async Task GetUserApplicationsByOrganisation_ShouldReturnPaginatedList_WhenSearchFiltersSet(string applicationType, string organisationName)
        {
            // Arrange
            var testOrgAppList = _fixture.Create<PaginatedList<OrganisationApplications>>();
            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.GetUserApplicationsByOrganisation(applicationType, organisationName, 1);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task UpdateEnrolment_SuccessfulResponse_ReturnsSuccess()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.UpdateEnrolment(new UpdateEnrolment());

            // Assert
            result.Should().Be(EndpointResponseStatus.Success);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task UpdateEnrolment_UnsuccessfulResponse_ReturnsFail()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.UpdateEnrolment(new UpdateEnrolment());

            // Assert
            result.Should().Be(EndpointResponseStatus.Fail);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetTestMessageAsync_WhenNotHttpStatusCodeOK_Then()
        {
            // Arrange
            string testMessage = "Test Message";
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(testMessage),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            string result = await _facadeService.GetTestMessageAsync();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<string>();
            result.Should().Contain("StatusCode: 404");
            result.Should().Contain("ReasonPhrase: 'Not Found'");

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetOrganisationEnrolments_WhenHttpStatusCodeOK_ThenReturnValidData()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            string organisationName = "Test Organisation";

            var enrolments = new OrganisationEnrolments
            {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
            };

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(enrolments)),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.GetOrganisationEnrolments(organisationId);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<OrganisationEnrolments>();
            Assert.AreEqual(expected: organisationId, actual: result.OrganisationId);
            Assert.AreEqual(expected: organisationName, actual: result.OrganisationName);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task GetOrganisationEnrolments_WhenNotHttpStatusCodeOK_ThenThrowException()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.GetOrganisationEnrolments(organisationId);

            // Assert
            Assert.IsNull(result);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public async Task GetOrganisationEnrolments_WhenMissingBaseAddress_ThenThrowException()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            _httpClient.BaseAddress = null;
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig, _mockLogger.Object);

            // Act
            var result = await _facadeService.GetOrganisationEnrolments(organisationId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public async Task TransferOrganisationNation_WhenMissingBaseAddress_ThenThrowException()
        {
            // Arrange
            var organisationNationTransfer = new OrganisationTransferNationRequest();

            _httpClient.BaseAddress = null;
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig, _mockLogger.Object);

            // Act
            var result = await _facadeService.TransferOrganisationNation(organisationNationTransfer);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task TransferOrganisationNation_WhenHttpStatusCodeNoContent_ThenReturnSuccess()
        {
            // Arrange
            var organisationNationTransfer = new OrganisationTransferNationRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.TransferOrganisationNation(organisationNationTransfer);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TransferOrganisationNation_WhenNotHttpStatusCodeNoContent_ThenThrowException()
        {
            // Arrange
            var organisationNationTransfer = new OrganisationTransferNationRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.TransferOrganisationNation(organisationNationTransfer);

            // Assert
            Assert.IsNull(result);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task SendEnrolmentEmails_WhenHttpStatusCodeOk_ThenReturnSuccess()
        {
            // Arrange
            var request = new EnrolmentDecisionRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.SendEnrolmentEmails(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task SendEnrolmentEmails_WhenHttpStatusCodeNotNoContent_ThenReturnFail()
        {
            // Arrange
            var request = new EnrolmentDecisionRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.SendEnrolmentEmails(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Fail);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task EnrolInvitedUser_WhenHttpStatusCodeNoContent_ThenReturnSuccess()
        {
            // Arrange
            var request = new EnrolInvitedUserRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.EnrolInvitedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task EnrolInvitedUser_WhenHttpStatusCodeNotNoContent_ThenReturnFail()
        {
            // Arrange
            var request = new EnrolInvitedUserRequest();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.EnrolInvitedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Fail);

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetUserAccountDetails_WhenNoSpecificRequirements_ThenReturnOkResult()
        {
            // Arrange
            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.GetUserAccountDetails();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<HttpResponseMessage>();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.ReasonPhrase.Should().Be("OK");

            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetOrganisationPomSubmissions_ShouldReturnPaginatedList_WhenRequestIsCorrect()
        {
            // Arrange
            var testOrgAppList = _fixture.Create<PaginatedList<Submission>>();
            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.GetOrganisationSubmissions<Submission>(null, null, null, null, null, null, 1);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);
            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("PomSubmissions")),
                    ItExpr.IsAny<CancellationToken>());

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetOrganisationRegistrationSubmissions_ShouldReturnPaginatedList_WhenRequestIsCorrect()
        {
            // Arrange
            var testOrgAppList = _fixture.Create<PaginatedList<Registration>>();
            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var result = await _facadeService.GetOrganisationSubmissions<Registration>(null,
                null,
                null,
                null,
                null,
                null,
                1);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);
            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("RegistrationSubmissions")),
                    ItExpr.IsAny<CancellationToken>());

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetOrganisationSubmissions_ParamsAdded_ShouldPassToFacade()
        {
            // Arrange
            var testOrgAppList = _fixture.Create<PaginatedList<Registration>>();
            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            string[] statuses = new[] { "Pending", "Accepted" };
            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            // Act
            var result =
                await _facadeService.GetOrganisationSubmissions<Registration>("orgName", "orgRef", OrganisationType.ComplianceScheme, statuses, submissionYears, submissionPeriods, 2);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);

            string expectedQueryString =
                "pageNumber=2&pageSize=10&organisationName=orgName&organisationReference=orgRef&organisationType=ComplianceScheme&statuses=Pending%2CAccepted&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024";

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryString)),
                    ItExpr.IsAny<CancellationToken>());

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task GetRegistrationSubmissionsCsv_ParamsAdded_ShouldPassToFacadeAndReturnCsv()
        {
            // Arrange

            var registration = _fixture.Create<Registration>();
            var testOrgAppList = new PaginatedList<Registration>
            {
                currentPage = 1,
                pageSize = 200,
                totalItems = 1,
                items = new List<Registration> { registration }
            };

            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            using var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            string organisationType = registration.OrganisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme";

            string expectedQueryString = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=ComplianceScheme&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024";
            string expectedCsv = $"organisation,organisation_id,submission_date_and_time,submission_period,status\r\n{registration.OrganisationName} ({organisationType}),{registration.OrganisationReference},{registration.RegistrationDate:d MMMM yyyy HH:mm:ss},{registration.SubmissionPeriod},{registration.Decision}\r\n";

            // Act

            var result = await _facadeService.GetRegistrationSubmissionsCsv(new GetRegistrationSubmissionsCsvRequest
            {
                SearchOrganisationName = "orgName",
                SearchOrganisationId = "orgRef",
                IsComplianceSchemeChecked = true,
                IsPendingRegistrationChecked = true,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = true,
                SearchSubmissionYears = submissionYears,
                SearchSubmissionPeriods = submissionPeriods
            });

            using var reader = new StreamReader(result);
            string csvString = await reader.ReadToEndAsync();

            // Assert

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryString)),
                    ItExpr.IsAny<CancellationToken>());

            csvString.Should().Be(expectedCsv);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissionsCsv_ParamsAdded_WithDirectProducerChecked_ShouldPassToFacadeAndReturnCsv()
        {
            // Arrange

            var registration = _fixture.Create<Registration>();
            var testOrgAppList = new PaginatedList<Registration>
            {
                currentPage = 1,
                pageSize = 200,
                totalItems = 1,
                items = new List<Registration> { registration }
            };

            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            using var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            string organisationType = registration.OrganisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme";

            string expectedQueryString = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=DirectProducer&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024";
            string expectedCsv = $"organisation,organisation_id,submission_date_and_time,submission_period,status\r\n{registration.OrganisationName} ({organisationType}),{registration.OrganisationReference},{registration.RegistrationDate:d MMMM yyyy HH:mm:ss},{registration.SubmissionPeriod},{registration.Decision}\r\n";

            // Act

            var result = await _facadeService.GetRegistrationSubmissionsCsv(new GetRegistrationSubmissionsCsvRequest
            {
                SearchOrganisationName = "orgName",
                SearchOrganisationId = "orgRef",
                IsDirectProducerChecked = true,
                IsPendingRegistrationChecked = true,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = true,
                SearchSubmissionYears = submissionYears,
                SearchSubmissionPeriods = submissionPeriods
            });

            using var reader = new StreamReader(result);
            string csvString = await reader.ReadToEndAsync();

            // Assert

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryString)),
                    ItExpr.IsAny<CancellationToken>());

            csvString.Should().Be(expectedCsv);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissionsCsv_ParamsAdded_WithNoOrganisationTypeFilter_ShouldPassToFacadeAndReturnCsv()
        {
            // Arrange

            var registration = _fixture.Create<Registration>();
            var testOrgAppList = new PaginatedList<Registration>
            {
                currentPage = 1,
                pageSize = 200,
                totalItems = 1,
                items = new List<Registration> { registration }
            };

            string jsonContent = JsonSerializer.Serialize(testOrgAppList);

            using var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            string organisationType = registration.OrganisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme";

            string expectedQueryString = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=All&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024";
            string expectedCsv = $"organisation,organisation_id,submission_date_and_time,submission_period,status\r\n{registration.OrganisationName} ({organisationType}),{registration.OrganisationReference},{registration.RegistrationDate:d MMMM yyyy HH:mm:ss},{registration.SubmissionPeriod},{registration.Decision}\r\n";

            // Act

            var result = await _facadeService.GetRegistrationSubmissionsCsv(new GetRegistrationSubmissionsCsvRequest
            {
                SearchOrganisationName = "orgName",
                SearchOrganisationId = "orgRef",
                IsPendingRegistrationChecked = true,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = true,
                SearchSubmissionYears = submissionYears,
                SearchSubmissionPeriods = submissionPeriods
            });

            using var reader = new StreamReader(result);
            string csvString = await reader.ReadToEndAsync();

            // Assert

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryString)),
                    ItExpr.IsAny<CancellationToken>());

            csvString.Should().Be(expectedCsv);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissionsCsv_ParamsAdded_WithMultiplePages_ShouldPassToFacadeAndReturnCsv()
        {
            // Arrange

            var registrations = _fixture
                .Build<Registration>()
                .With(x => x.OrganisationType, OrganisationType.DirectProducer)
                .CreateMany(2)
                .ToList();

            var testOrgAppListPage1 = new PaginatedList<Registration>
            {
                currentPage = 1,
                pageSize = 1,
                totalItems = registrations.Count,
                items = new List<Registration> { registrations[0] }
            };

            var testOrgAppListPage2 = new PaginatedList<Registration>
            {
                currentPage = 2,
                pageSize = 1,
                totalItems = registrations.Count,
                items = new List<Registration> { registrations[1] }
            };

            string jsonContentPage1 = JsonSerializer.Serialize(testOrgAppListPage1);
            string jsonContentPage2 = JsonSerializer.Serialize(testOrgAppListPage2);

            using var httpResponseMessagePage1 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContentPage1, Encoding.UTF8, "application/json"),
            };
            using var httpResponseMessagePage2 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContentPage2, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessagePage1)
                .ReturnsAsync(httpResponseMessagePage2);

            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            string organisationType = registrations[0].OrganisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme";

            string expectedQueryStringPage1 = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=ComplianceScheme&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024&pageNumber=1";
            string expectedQueryStringPage2 = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=ComplianceScheme&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024&pageNumber=2";
            string expectedCsv = $"organisation,organisation_id,submission_date_and_time,submission_period,status\r\n{registrations[0].OrganisationName} ({organisationType}),{registrations[0].OrganisationReference},{registrations[0].RegistrationDate:d MMMM yyyy HH:mm:ss},{registrations[0].SubmissionPeriod},{registrations[0].Decision}\r\n{ registrations[1].OrganisationName} ({organisationType}),{registrations[1].OrganisationReference},{registrations[1].RegistrationDate:d MMMM yyyy HH:mm:ss},{registrations[1].SubmissionPeriod},{registrations[1].Decision}\r\n";

            // Act

            var result = await _facadeService.GetRegistrationSubmissionsCsv(new GetRegistrationSubmissionsCsvRequest
            {
                SearchOrganisationName = "orgName",
                SearchOrganisationId = "orgRef",
                IsComplianceSchemeChecked = true,
                IsPendingRegistrationChecked = true,
                IsAcceptedRegistrationChecked = true,
                IsRejectedRegistrationChecked = true,
                SearchSubmissionYears = submissionYears,
                SearchSubmissionPeriods = submissionPeriods
            });

            using var reader = new StreamReader(result);
            string csvString = await reader.ReadToEndAsync();

            // Assert

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryStringPage1)),
                    ItExpr.IsAny<CancellationToken>());

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("RegistrationSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryStringPage2)),
                    ItExpr.IsAny<CancellationToken>());

            csvString.Should().Be(expectedCsv);
        }

        [TestMethod]
        public async Task GetPackagingSubmissionsCsv_ParamsAdded_ShouldPassToFacadeAndReturnCsv()
        {
            // Arrange

            var submission = _fixture.Create<Submission>();
            var testPomAppList = new PaginatedList<Submission>
            {
                currentPage = 1,
                pageSize = 200,
                totalItems = 1,
                items = new List<Submission> { submission }
            };

            string jsonContent = JsonSerializer.Serialize(testPomAppList);

            using var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json"),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            int[] submissionYears = new[] { 2023, 2024 };
            string[] submissionPeriods = new[] { "January to June 2023", "January to June 2024" };

            string organisationType = submission.OrganisationType == OrganisationType.DirectProducer ? "Direct Producer" : "Compliance Scheme";

            string expectedQueryString = "pageSize=200&organisationName=orgName&organisationReference=orgRef&organisationType=ComplianceScheme&statuses=Pending%2CAccepted%2CRejected&submissionYears=2023%2C2024&submissionPeriods=January%20to%20June%202023%2CJanuary%20to%20June%202024";
            string expectedCsv = $"organisation,organisation_id,submission_date_and_time,submission_period,status\r\n{submission.OrganisationName} ({organisationType}),{submission.OrganisationReference},{submission.SubmittedDate:d MMMM yyyy HH:mm:ss},{submission.ActualSubmissionPeriod},{submission.Decision}\r\n";

            // Act

            var result = await _facadeService.GetPackagingSubmissionsCsv(new GetPackagingSubmissionsCsvRequest
            {
                SearchOrganisationName = "orgName",
                SearchOrganisationId = "orgRef",
                IsComplianceSchemeChecked = true,
                IsPendingSubmissionChecked = true,
                IsAcceptedSubmissionChecked = true,
                IsRejectedSubmissionChecked = true,
                SearchSubmissionYears = submissionYears,
                SearchSubmissionPeriods = submissionPeriods
            });

            using var reader = new StreamReader(result);
            string csvString = await reader.ReadToEndAsync();

            // Assert

            _mockHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.RequestUri.AbsoluteUri.Contains("PomSubmissions")
                         && req.RequestUri.AbsoluteUri.Contains(expectedQueryString)),
                    ItExpr.IsAny<CancellationToken>());

            csvString.Should().Be(expectedCsv);
        }

        [TestMethod]
        public async Task SubmitPomSubmission_WhenHttpStatusCodeOk_ThenReturnSuccess()
        {
            // Arrange
            var request = new RegulatorPoMDecisionCreateRequest();

            using var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.SubmitPoMDecision(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);
        }

        [TestMethod]
        public async Task GetOrganisationBySearchTerm_WhenNoSpecificRequirements_ThenReturnOkResult()
        {
            var pagedOrganisationResults = new PaginatedList<OrganisationSearchResult>
            {
                currentPage = 1,
                pageSize = 10,
                totalItems = 0,
                items = new List<OrganisationSearchResult>()
            };

            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(pagedOrganisationResults))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _facadeService.GetOrganisationBySearchTerm("searchTerm");

            // Assert
            Assert.IsNotNull(result);

            response.Dispose();
        }

        [TestMethod]
        public async Task GetRegulatorCompanyDetails_WhenDataPassed_ThenReturnOkResult()
        {
            var companyDetailsResult = new RegulatorCompanyDetailsModel()
            {
                Company = new Company()
                {
                    OrganisationId = Guid.NewGuid().ToString(),
                    OrganisationName = "Test Company"
                },
                CompanyUserInformation = new List<CompanyUserInformation>()
            };

            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(companyDetailsResult))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _facadeService.GetRegulatorCompanyDetails(_organisationId);

            // Assert
            Assert.IsNotNull(result);

            response.Dispose();
        }

        [TestMethod]
        public async Task GetRegulatorCompanyDetails_WhenHttpStatusCodeOK_ThenReturnValidData()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(MockedOrganisationDetails.GetMockedOrganisationDetails())),
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.GetRegulatorCompanyDetails(organisationId);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RegulatorCompanyDetailsModel>();
            httpTestHandler.Dispose();
        }

        [TestMethod]
        public async Task GetProducerOrganisationUsersByOrganisationExternalId_WhenHttpStatusCodeOK_ThenReturnValidData()
        {
            var organisationUsers = new List<OrganisationUser>
            {
                new() {FirstName = "James", LastName = "Smith", Email = "jsmith@gmail.com", PersonExternalId = Guid.NewGuid()},
                new() {FirstName = "John", LastName = "Lewis", Email = "jlewis@gmail.com", PersonExternalId = Guid.NewGuid()},
            };

            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(organisationUsers))
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _facadeService.GetProducerOrganisationUsersByOrganisationExternalId(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeEquivalentTo(organisationUsers);

            response.Dispose();
        }

        [TestMethod]
        public async Task RemoveApprovedUser_WhenHttpStatusCodeOk_ThenReturnSuccess()
        {
            // Arrange
            using var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var request = new RemoveApprovedUserRequest
            {
                RemovedConnectionExternalId = _connExternalId,
                OrganisationId = _organisationId,
                PromotedPersonExternalId = _promotedPersonExternalId
            };

            // Act
            var result = await _facadeService.RemoveApprovedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);
        }

        [TestMethod]
        public async Task RemoveApprovedUser_UnsuccessfulResponse_ReturnsFail()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var request = new RemoveApprovedUserRequest
            {
                RemovedConnectionExternalId = _connExternalId,
                OrganisationId = _organisationId,
                PromotedPersonExternalId = _promotedPersonExternalId
            };
            // Act
            var result = await _facadeService.RemoveApprovedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Fail);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task AddRemoveApprovedUser_WhenHttpStatusCodeNoContent_ThenReturnSuccess()
        {
            // Arrange
            var request = new AddRemoveApprovedUserRequest();

            using var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _facadeService.AddRemoveApprovedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Success);
        }

        [TestMethod]
        public async Task AddRemoveApprovedUser_WhenHttpStatusCodeBadRequest_ThenReturnFail()
        {
            // Arrange
            var request = new AddRemoveApprovedUserRequest();

            using var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _facadeService.AddRemoveApprovedUser(request);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Fail);
        }

        [TestMethod]
        public async Task SubmitRegistrationDecision_WhenHttpStatusCodeOk_ThenReturnSuccess()
        {
            // Arrange
            var request = new RegulatorRegistrationDecisionCreateRequest();

            using var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.SubmitRegistrationDecision(request);

            // Assert
            result.Should().Be(EndpointResponseStatus.Success);
        }

        [TestMethod]
        public async Task SubmitRegistrationDecision_WhenHttpStatusCodeBadRequest_ThenReturnFail()
        {
            // Arrange
            var request = new RegulatorRegistrationDecisionCreateRequest();

            using var httpTestHandler = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpTestHandler);

            // Act
            var result = await _facadeService.SubmitRegistrationDecision(request);

            // Assert
            result.Should().Be(EndpointResponseStatus.Fail);
        }

        [TestMethod]
        public async Task GetFileDownload_ReturnsHttpResponseMessage()
        {
            // Arrange
            using var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("This is a mock file content")
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri == new Uri("https://api.example.com/file/download")
                    ),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            var request = new FileDownloadRequest
            {
                SubmissionId = Guid.Parse("9bdd8616-9155-438b-a00c-32b38e6b9aa5"),
                FileId = Guid.Parse("f1e85000-165a-42c4-9d9d-48bc692a1e10"),
                BlobName = "e1fb01bb-45ee-4e1e-a21d-788b8c140b42",
                FileName = "RegData_OrgType_LLP.csv",
                SubmissionType = SubmissionType.Registration
            };

            // Act
            var response = await _facadeService.GetFileDownload(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("This is a mock file content", content);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://api.example.com/file/download")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [TestMethod]
        public void PrepareAuthenticateClient_WillContructURL_When_HttpClientIsNull()
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            using var httpClient = new HttpClient(mockHandler.Object);
            var facadeApiConfig = Options.Create(new FacadeApiConfig
            {
                BaseUrl = "http://localhost/"
            });

            var sut = new FacadeService(httpClient, _tokenAcquisitionMock.Object, _paginationConfig, facadeApiConfig, _mockLogger.Object);
            sut.GetRegistrationSubmissions(new RegistrationSubmissionsFilterModel { PageNumber = 1 });
            httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        }

        [TestMethod]
        public async Task SubmitRegulatorRegistrationDecisionAsync_ReturnsSuccess_WhenResponseIsSuccessful()
        {
            // Arrange
            var request = _fixture.Create<RegulatorDecisionRequest>();
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
            var result = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(request);

            // Assert
            Assert.AreEqual(EndpointResponseStatus.Success, result);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("organisation-registration-submission-decision")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task SubmitRegulatorRegistrationDecisionAsync_ReturnsFail_WhenResponseIsUnsuccessful()
        {
            // Arrange
            var request = _fixture.Create<RegulatorDecisionRequest>();

            string jsonRequest = JsonSerializer.Serialize(new ProblemDetails { Status = 400 });
            var stringContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = stringContent
                })
                .Verifiable();

            // Act
            var result = await _facadeService.SubmitRegulatorRegistrationDecisionAsync(request);

            // Assert
            Assert.AreEqual(EndpointResponseStatus.Fail, result);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("organisation-registration-submission-decision")),
                ItExpr.IsAny<CancellationToken>());
            stringContent?.Dispose();
        }


        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public async Task GetRegistrationSubmissionDetails_WhenMissingBaseAddress_ThenThrowException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            _httpClient.BaseAddress = null;
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig, _mockLogger.Object);

            // Act
            var result = await _facadeService.GetRegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [DataRow(HttpStatusCode.OK, EndpointResponseStatus.Success)]
        [DataRow(HttpStatusCode.BadRequest, EndpointResponseStatus.Fail)]
        [DataRow(HttpStatusCode.InternalServerError, EndpointResponseStatus.Fail)]
        [DataRow(HttpStatusCode.ServiceUnavailable, EndpointResponseStatus.Fail)]
        public async Task SubmitRegistrationFeePaymentAsync_Returns_Correct_Status_BasedOn_Response(HttpStatusCode statusCode, EndpointResponseStatus expectedStatus)
        {
            // Arrange

            StringContent stringContent = null;

            if (statusCode == HttpStatusCode.BadRequest)
            {
                string jsonRequest = JsonSerializer.Serialize(new ValidationProblemDetails { Status = 400 });
                stringContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            }
            else if (statusCode != HttpStatusCode.OK)
            {
                string jsonRequest = JsonSerializer.Serialize(new ProblemDetails { Status = 500 });
                stringContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            }

            var request = _fixture.Create<RegistrationFeePaymentRequest>();
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = stringContent
                })
                .Verifiable();

            // Act
            await _facadeService.SubmitRegistrationFeePaymentAsync(request);

            // Assert
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("organisation-registration-fee-payment")),
                ItExpr.IsAny<CancellationToken>());

            stringContent?.Dispose();
        }

        [TestMethod]
        public async Task GetRegistrationSubmissions_Success_ReturnsPaginatedList()
        {
            // Arrange
            var filter = new RegistrationSubmissionsFilterModel();
            var responseObject = new
            {
                items = new[] { new { field = "value" } },
                currentPage = 2,
                totalItems = 50,
                pageSize = 10
            };

            string responseContent = JsonSerializer.Serialize(responseObject);

            _mockHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
               {
                   Assert.AreEqual(HttpMethod.Post, request.Method);
                   Assert.AreEqual($"http://localhost/organisation-registration-submissions", request.RequestUri.ToString());

                   return new HttpResponseMessage(HttpStatusCode.OK)
                   {
                       Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                   };
               });
            // Act
            var result = await _facadeService.GetRegistrationSubmissions(filter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.items.Count);
            Assert.AreEqual(2, result.currentPage);
            Assert.AreEqual(50, result.totalItems);
            Assert.AreEqual(10, result.pageSize);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissions_Failure_ReturnsDefaultPaginatedList()
        {
            // Arrange
            var filter = new RegistrationSubmissionsFilterModel();

            _mockHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act
            var result = await _facadeService.GetRegistrationSubmissions(filter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.totalItems);
            Assert.AreEqual(1, result.currentPage);
            Assert.AreEqual(20, result.pageSize); // default page size in failure case
            Assert.AreEqual(0, result.items.Count);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissions_ClientErrorStatusCode_ReturnsDefaultPaginatedList()
        {
            // Arrange
            var filter = new RegistrationSubmissionsFilterModel();

            _mockHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await _facadeService.GetRegistrationSubmissions(filter);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.totalItems);
            Assert.AreEqual(1, result.currentPage);
            Assert.AreEqual(20, result.pageSize);
            Assert.AreEqual(0, result.items.Count);
        }

        [TestMethod]
        public async Task GetRegistrationSubmissionDetails_Success_ReturnsObject()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var expectedResult = new RegistrationSubmissionOrganisationDetailsResponse
            {
                ApplicationReferenceNumber = "TEST"
            };

            string json = JsonSerializer.Serialize(expectedResult);

            _mockHandler.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new StringContent(json, Encoding.UTF8, "application/json")
               });

            // Act
            var result = await _facadeService.GetRegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TEST", ((RegistrationSubmissionOrganisationDetails)result).ApplicationReferenceNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task GetRegistrationSubmissionDetails_NonSuccess_ThrowsHttpRequestException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            _mockHandler.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new StringContent("", Encoding.UTF8, "application/json")
               });

            // Act
            // EnsureSuccessStatusCode will throw HttpRequestException
            var result = await _facadeService.GetRegistrationSubmissionDetails(submissionId);

            // Assert handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task GetRegistrationSubmissionDetails_InvalidJson_ThrowsInvalidDataException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            // Malformed JSON
            var malformedJson = "{ \"Invalid\": ";
            _mockHandler.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
               {
                   Content = new StringContent(malformedJson, Encoding.UTF8, "application/json")
               });

            // Act
            // ConvertCommonDataToFE should throw a JsonException due to malformed JSON
            var result = await _facadeService.GetRegistrationSubmissionDetails(submissionId);

            // Assert handled by ExpectedException
        }


        [TestMethod]
        public async Task ReadRequiredJsonContent_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var json = @"
            {
                ""items"": [
                    { ""Field"": ""Value1"" },
                    { ""Field"": ""Value2"" }
                ],
                ""currentPage"": 2,
                ""totalItems"": 100,
                ""pageSize"": 10
            }";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var result = await FacadeService.ReadRequiredJsonContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.items.Count);
            Assert.AreEqual(2, result.currentPage);
            Assert.AreEqual(100, result.totalItems);
            Assert.AreEqual(10, result.pageSize);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task ReadRequiredJsonContent_MalformedJson_ThrowsInvalidDataException()
        {
            // Arrange
            // Missing a closing brace or quotes to simulate malformed JSON
            var json = @"{ ""items"": [ { ""Field"": ""Value"" } ";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            // This should throw an InvalidDataException due to the catch block
            var result = await FacadeService.ReadRequiredJsonContent(content);

            // Assert handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task ReadRequiredJsonContent_EmptyJson_ThrowsInvalidDataException()
        {
            // Arrange
            var json = ""; // empty string
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            // Empty JSON won't deserialize properly and should raise the exception
            var result = await FacadeService.ReadRequiredJsonContent(content);

            // Assert handled by ExpectedException
        }

        [TestMethod]
        public async Task ReadRequiredJsonContent_InvalidStructure_Returns_Empty_PaginatedDate()
        {
            // Arrange
            // This is valid JSON but doesn't match the expected structure (e.g., missing required fields)
            var json = @"{ ""notItems"": [ { ""Field"": ""Value"" } ] }";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var result = await FacadeService.ReadRequiredJsonContent(content);

            // Assert handled by ExpectedException
            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedList<OrganisationRegistrationSubmissionSummaryResponse>>();
        }


        public static class PaginatedListHelper
        {
            public static PaginatedList<T> Create<T>(List<T> items, int totalCount, int currentPage, int pageSize) => new PaginatedList<T>
            {
                items = items,
                totalItems = totalCount,
                currentPage = currentPage,
                pageSize = pageSize
            };
        }
    }
}