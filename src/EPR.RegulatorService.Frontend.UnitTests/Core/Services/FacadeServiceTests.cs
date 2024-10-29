using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.MockedData.Filters;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Services;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moq.Protected;

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services
{
    [TestClass]
    public class FacadeServiceTests
    {
        private const string TestOrgName = "Test org name";
        private const string ApprovedPerson = "ApprovedPerson";
        private const string DelegatedPerson = "DelegatedPerson";
        private Mock<HttpMessageHandler> _mockHandler;
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
                    ["OrganisationRegistrationSubmissions"] = "registrations/get-organisations&currentPage={0}&pageSize={1}"
                },
                DownstreamScope = "api://default"
            });

            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig);
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
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig);

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
            _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig);

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
                CurrentPage = 1,
                PageSize = 10,
                TotalItems = 0,
                Items = new List<OrganisationSearchResult>()
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

            var sut = new FacadeService(httpClient, _tokenAcquisitionMock.Object, _paginationConfig, facadeApiConfig);
            sut.GetRegistrationSubmissions(new RegistrationSubmissionsFilterModel { Page = 1 });
            httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetRegistrationSubmissions_ShouldReturnPaginatedList_WhenRequestSucceeds()
        {
            var filter = new RegistrationSubmissionsFilterModel { Page = 1 };

            var result = await _facadeService.GetRegistrationSubmissions(filter);
            result.Should().BeOfType<PaginatedList<RegistrationSubmissionOrganisationDetails>>();
            result.Items.Should().HaveCount(PAGE_SIZE);
        }

        [TestMethod]
        public async Task GetRegistrationSubmission_ShouldReturnPageTwo_WhenRequestSucceeds()
        {
            var result = await _facadeService.GetRegistrationSubmissions(new RegistrationSubmissionsFilterModel { Page = 2 });
            result.CurrentPage.Should().Be(2);
        }

        [TestMethod]
        [DataRow(24)]
        [DataRow(90)]
        public async Task GetRegisrationSubmission_FilterByOrgName_ShouldReturnSuccess_And_1Org(int byIndex)
        {
            var expectedDataSet = MockedFacadeService.GenerateRegistrationSubmissionDataCollection();
            var expectedResult = expectedDataSet[byIndex];

            var filter = new RegistrationSubmissionsFilterModel() { Page = 1, OrganisationName = expectedDataSet[byIndex].OrganisationName };
            var results = await _facadeService.GetRegistrationSubmissions(filter);

            results.Items.Should().Contain(expectedResult);
        }

        [TestMethod]
        [DataRow(24)]
        [DataRow(90)]
        public async Task GetRegistrationSubmission_WithFilter_ShouldReturnCorrectPaginationInformation_1Page(int byIndex)
        {
            var expectedDataSet = MockedFacadeService.GenerateRegistrationSubmissionDataCollection();
            var expectedResult = expectedDataSet[byIndex];

            var filter = new RegistrationSubmissionsFilterModel() { Page = 1, OrganisationName = expectedResult.OrganisationName };
            var results = await _facadeService.GetRegistrationSubmissions(filter);

            results.TotalPages.Should().Be(1);
        }

        [TestMethod]
        [DataRow(24)]
        [DataRow(90)]
        public async Task GetRegistrationSubmission_WithFilter_ShouldReturnCorrectPaginationInformation_MoreThan1Page(int byIndex)
        {
            var expectedDataSet = MockedFacadeService.GenerateRegistrationSubmissionDataCollection();
            var expectedResult = expectedDataSet[byIndex];

            var filter = new RegistrationSubmissionsFilterModel() { Page = 1, OrganisationType = expectedResult.OrganisationType.ToString()};
            var results = await _facadeService.GetRegistrationSubmissions(filter);

            results.TotalPages.Should().BeGreaterThan(1);
            results.TotalItems.Should().BeGreaterThan(1);
        }

        [TestMethod]
        public async Task GetRegistrationSubmission_WithoutFilter_ShouldReturnCorrectPaginationInformation()
        {
            var filter = new RegistrationSubmissionsFilterModel() { Page = 1 };
            var results = await _facadeService.GetRegistrationSubmissions(filter);

            Assert.AreNotEqual(results.Items.Count, results.TotalItems);
        }

        [TestMethod]
        [DataRow(14)]
        [DataRow(45)]
        public async Task GetRegisrationSubmission_FilterByOrgRef_ShouldReturnSuccess_And_1Org(int byIndex)
        {
            var expectedDataSet = MockedFacadeService.GenerateRegistrationSubmissionDataCollection();
            var expectedResult = expectedDataSet[byIndex];

            var filter = new RegistrationSubmissionsFilterModel() { Page = 1, OrganisationRef = expectedDataSet[byIndex].OrganisationReference };
            var results = await _facadeService.GetRegistrationSubmissions(filter);

            results.Items.Should().Contain(expectedResult);
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(15)]
        [DataRow(19)]
        [DataRow(23)]
        [DataRow(44)]
        public async Task FilterByNameSizeStatusAndYear_ReturnsTheCorrectSet(int byIndex)
        {
            var item = MockedFacadeService.GenerateRegistrationSubmissionDataCollection()[byIndex];
            var expectedItems = new List<RegistrationSubmissionOrganisationDetails>
                                {
                                    item
                                };

            string expectedName = item.OrganisationName[3..6];
            var expectedSize = item.OrganisationType;
            var expectedStatus = item.RegistrationStatus;
            string expectedYear = item.RegistrationYear.ToString(CultureInfo.InvariantCulture);

            var filter = new RegistrationSubmissionsFilterModel
            {
                OrganisationName = expectedName,
                OrganisationType = expectedSize.ToString(),
                SubmissionStatus = expectedStatus.ToString(),
                RelevantYear = expectedYear,
                PageSize=5000
            };

            var result = await _facadeService.GetRegistrationSubmissions(filter);
            result.Items.Should().Contain(expectedItems);
        }

        [TestMethod]
        [DataRow(53, 66)]
        [DataRow(15, 117)]
        [DataRow(115, 76)]
        public async Task FilterByMultipleNameSizeStatusAndYear_ReturnsACompleteSet(int byIndex, int byOtherIndex)
        {
            var alldata = MockedFacadeService.GenerateRegistrationSubmissionDataCollection();

            var item = alldata[byIndex];
            var item2 = alldata[byOtherIndex];

            var expectedItems = alldata.AsQueryable().FilterByOrganisationType($"{item.OrganisationType.ToString()} {item2.OrganisationType.ToString()}")
                                                     .FilterBySubmissionStatus($"{item.RegistrationStatus.ToString()} {item2.RegistrationStatus.ToString()}")
                                                     .FilterByRelevantYear($"{item.RegistrationYear} {item2.RegistrationYear}")
                                                     .OrderBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.refused)
                                                     .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.granted)
                                                     .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.cancelled)
                                                     .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.updated)
                                                     .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.queried)
                                                     .ThenBy(x => x.RegistrationStatus == RegistrationSubmissionStatus.pending)
                                                     .ThenBy(x => x.RegistrationDateTime)
                                                     .Skip((1 - 1) * PAGE_SIZE)
                                                     .Take(PAGE_SIZE)
                                                     .ToList();

            var expectedSize1 = item.OrganisationType;
            var expectedStatus1 = item.RegistrationStatus;
            string expectedYear1 = item.RegistrationYear;
            var expectedSize2 = item2.OrganisationType;
            var expectedStatus2 = item2.RegistrationStatus;
            string expectedYear2 = item2.RegistrationYear;

            var filter = new RegistrationSubmissionsFilterModel
            {
                OrganisationType = $"{expectedSize1.ToString()} {expectedSize2.ToString()}",
                SubmissionStatus = $"{expectedStatus1.ToString()} {expectedStatus2.ToString()}",
                RelevantYear = $"{expectedYear1} {expectedYear2}"
            };

            var result = await _facadeService.GetRegistrationSubmissions(filter);
            result.Items.Should().BeEquivalentTo(expectedItems);
        }
    }
}