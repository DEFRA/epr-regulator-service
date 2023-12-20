using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Services;
using AutoFixture;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Net;
using System.Text.Json;
using System.Text;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;


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

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            _httpClient = new HttpClient(_mockHandler.Object){BaseAddress = new Uri("http://localhost")};
            _paginationConfig = Options.Create(new PaginationConfig { PageSize = 10 });
            _facadeApiConfig = Options.Create(new FacadeApiConfig
            {
                Endpoints = new Dictionary<string, string>
                {
                    ["PendingApplications"] = "http://testurl.com",
                    ["UpdateEnrolment"] = "http://testurl.com",
                    ["OrganisationEnrolmentsPath"] = "organisations/{0}/pending-applications",
                    ["OrganisationTransferNationPath"]= "accounts/transfer-nation",
                    ["SendEnrolmentUpdatedEmails"] = "regulators/accounts/govNotification",
                    ["EnrolInvitedUser"] = "accounts-management/enrol-invited-user",
                    ["UserAccounts"] = "user-accounts",
                    ["OrganisationsSearchPath"] = "organisations/search-organisations?currentPage={0}&pageSize={1}&searchTerm={2}",
                    ["OrganisationDetails"] = "organisations/organisation-details?externalId={0}",
                    ["GetOrganisationUsersByOrganisationExternalIdPath"] = "organisations/users-by-organisation-external-id?externalId={0}",
                    ["PomSubmissions"] = "http://testurl.com",
                    ["OrganisationsSearchPath"] = "organisations/search-organisations?currentPage={0}&pageSize={1}&searchTerm={2}",
                    ["PomSubmissionDecision"] = "http://testurl.com",
                    ["OrganisationsRemoveApprovedUser"] = "organisations/remove-approved-users?connExternalId={0}&organisationId={1}"
                }
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

            var httpResponseMessage = new HttpResponseMessage {
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
            var result = await _facadeService.GetTestMessageAsync();

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
        public async Task GetOrganisationSubmissions_ShouldReturnPaginatedList_WhenRequestIsCorrect()
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
            var result = await _facadeService.GetOrganisationSubmissions(null, null, null,null, 1);

            // Assert
            result.Should().BeEquivalentTo(testOrgAppList);

            httpResponseMessage.Dispose();
        }

        [TestMethod]
        public async Task SubmitPomSubmission_WhenHttpStatusCodeOk_ThenReturnSuccess()
        {
            // Arrange
            var request = new RegulatorPoMDecisionCreateRequest();

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
            var response = new HttpResponseMessage
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
            var result = await _facadeService.RemoveApprovedUser(_connExternalId,_organisationId);

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

            // Act
            var result = await _facadeService.RemoveApprovedUser(_connExternalId,_organisationId);

            // Assert
            Assert.IsNotNull(result);
            result.Should().Be(EndpointResponseStatus.Fail);

            httpResponseMessage.Dispose();
        }
    }
}