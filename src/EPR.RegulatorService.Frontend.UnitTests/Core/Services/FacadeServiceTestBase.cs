namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services;

public abstract class FacadeServiceTestBase
{
    private const int PageSize = 10;

    protected Mock<HttpMessageHandler> _mockHandler = null!;
    protected Mock<ILogger<FacadeService>> _mockLogger = null!;
    protected Mock<ITokenAcquisition> _tokenAcquisitionMock = null!;
    protected HttpClient _httpClient = null!;
    protected IOptions<PaginationConfig> _paginationConfig = null!;
    protected IOptions<FacadeApiConfig> _facadeApiConfig = null!;
    protected Fixture _fixture = null!;
    protected FacadeService _facadeService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _mockLogger = new Mock<ILogger<FacadeService>>();
        _tokenAcquisitionMock = new Mock<ITokenAcquisition>();
        _httpClient = new HttpClient(_mockHandler.Object) { BaseAddress = new Uri("http://localhost") };
        _paginationConfig = Options.Create(new PaginationConfig { PageSize = PageSize });
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
                ["OrganisationsRemoveApprovedUser"] = "organisations/remove-approved-users?connExternalId={0}&organisationId={1}    &promotedPersonExternalId=  {2}",
                ["AddRemoveApprovedUser"] = "/accounts-management/add-remove-approved-users",
                ["RegistrationSubmissionDecisionPath"] = "http://testurl.com",
                ["FileDownload"] = "https://api.example.com/file/download",
                ["OrganisationRegistrationSubmissions"] = "registrations/get-organisations&currentPage={0}&pageSize={1}",
                ["OrganisationRegistrationSubmissionDecisionPath"] = "organisation-registration-submission-decision",
                ["GetOrganisationRegistrationSubmissionDetailsPath"] = "registrations-submission-details/submissionId/{0}",
                ["GetOrganisationRegistrationSubmissionsPath"] = "organisation-registration-submissions",
                ["SubmitRegistrationFeePaymentPath"] = "organisation-registration-fee-payment",
                ["PackagingDataResubmissionFeePaymentPath"] = "organisation-packaging-data-resubmission-fee-payment",
                ["GetPomResubmissionPaycalParameters"] = "pom/get-resubmission-paycal-parameters",
            },
            DownstreamScope = "api://default"
        });

        _facadeService = new FacadeService(_httpClient, _tokenAcquisitionMock.Object, _paginationConfig, _facadeApiConfig, _mockLogger.Object);
        _fixture = new Fixture();
    }
}