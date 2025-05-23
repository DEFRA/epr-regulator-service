namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Services;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Sessions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public abstract class RegistrationSubmissionsTestBase
    {
        private const string BackLinkViewDataKey = "BackLinkToDisplay";
        protected Mock<IFacadeService> _facadeServiceMock = null!;
        protected Mock<IPaymentFacadeService> _paymentFacadeServiceMock = null!;

        protected Mock<ILogger<RegistrationSubmissionsController>> _loggerMock = null!;
        protected RegistrationSubmissionsController _controller = null!;
        protected Mock<HttpContext> _mockHttpContext = null!;
        protected Mock<IOptions<ExternalUrlsOptions>> _mockUrlsOptions = null!;
        protected Mock<IOptions<RegistrationSubmissionsOptions>> _mockRegistrationSubmissionOptions = null!;
        protected Mock<IConfiguration> _mockConfiguration = null!;
        protected Mock<ISessionManager<JourneySession>> _mockSessionManager = null!;
        protected JourneySession _journeySession;
        private const string PowerBiLogin = "https://app.powerbi.com/";
        protected Mock<IUrlHelper> _mockUrlHelper = null!;

        protected void SetupBase(bool? show2026RelevantYearFilter = false)
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockUrlsOptions = new Mock<IOptions<ExternalUrlsOptions>>();
            _mockRegistrationSubmissionOptions = new Mock<IOptions<RegistrationSubmissionsOptions>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<RegistrationSubmissionsController>>();
            _facadeServiceMock = new Mock<IFacadeService>();
            _paymentFacadeServiceMock = new Mock<IPaymentFacadeService>();
            _mockSessionManager = new Mock<ISessionManager<JourneySession>>();

            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(section => section.Value).Returns("/regulators");
            _mockConfiguration.Setup(config => config.GetSection(ConfigKeys.PathBase))
                .Returns(mockConfigurationSection.Object);

            _mockUrlsOptions.Setup(mockUrlOptions =>
                mockUrlOptions.Value)
                .Returns(new ExternalUrlsOptions
                {
                    PowerBiLogin = PowerBiLogin
                });

            _mockRegistrationSubmissionOptions.Setup(mockRegistrationSubmissionOptions =>
                mockRegistrationSubmissionOptions.Value)
                .Returns(new RegistrationSubmissionsOptions
                {
                    Show2026RelevantYearFilter = show2026RelevantYearFilter.Value
                });

            SetupJourneySession(null, null);

            _mockUrlHelper = new Mock<IUrlHelper>();
            _mockUrlHelper
                .Setup(helper => helper.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("/expected/backlink/url");

            _controller = new RegistrationSubmissionsController(
                _facadeServiceMock.Object,
                _paymentFacadeServiceMock.Object,
                _mockSessionManager.Object,
                _loggerMock.Object,
                _mockConfiguration.Object,
                _mockUrlsOptions.Object,
                _mockRegistrationSubmissionOptions.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                },
                Url = _mockUrlHelper.Object,
                TempData = new Mock<ITempDataDictionary>().Object
            };
        }

        public void SetupJourneySession(RegistrationSubmissionsFilterModel filtersModel,
                                        RegistrationSubmissionOrganisationDetails selectedSubmission,
                                        int currentPageNumber = 1)
        {
            _journeySession = new JourneySession()
            {
                UserData = new Common.Authorization.Models.UserData
                {
                    Id = Guid.NewGuid()
                },
                RegulatorRegistrationSubmissionSession = new()
                {
                    LatestFilterChoices = filtersModel,
                    CurrentPageNumber = currentPageNumber,
                    SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>()
                }
            };
            if (selectedSubmission is not null)
            {
                _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.Add(selectedSubmission.SubmissionId, selectedSubmission);
            }

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);
        }

        protected static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
        {
            bool hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
            hasBackLinkKey.Should().BeTrue();
            (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
        }

        protected static RegistrationSubmissionDetailsViewModel GenerateTestSubmissionDetailsViewModel(
            Guid organisationId,
            int nationId = 3,
            string nationCode = "Sco") => new RegistrationSubmissionDetailsViewModel
            {
                SubmissionId = organisationId,
                OrganisationId = organisationId,
                OrganisationReference = "215 148",
                OrganisationName = "Acme org Ltd.",
                RegistrationReferenceNumber = "REF001",
                ReferenceNumber = "REF002",
                OrganisationType = RegistrationSubmissionOrganisationType.large,
                BusinessAddress = new BusinessAddress
                {
                    BuildingName = string.Empty,
                    BuildingNumber = "10",
                    Street = "High Street",
                    County = "Randomshire",
                    PostCode = "A12 3BC"
                },
                CompaniesHouseNumber = "0123456",
                RegisteredNation = nationCode,
                NationId = nationId,
                PowerBiLogin = "https://app.powerbi.com/",
                Status = RegistrationSubmissionStatus.Queried,
                SubmissionDetails = new SubmissionDetailsViewModel
                {
                    Status = RegistrationSubmissionStatus.Queried,
                    LatestDecisionDate = new DateTime(2024, 10, 21, 16, 23, 42, DateTimeKind.Utc),
                    TimeAndDateOfSubmission = new DateTime(2024, 7, 10, 16, 23, 42, DateTimeKind.Utc),
                    SubmittedOnTime = true,
                    SubmittedBy = "Sally Smith",
                    AccountRole = Frontend.Core.Enums.ServiceRole.ApprovedPerson,
                    AccountRoleId = (int)Frontend.Core.Enums.ServiceRole.ApprovedPerson,
                    Telephone = "07553 937 831",
                    Email = "sally.smith@email.com",
                    DeclaredBy = "Sally Smith",
                    Files =
                    [
                    ],
                },
                ProducerComments = "producer comment",
                RegulatorComments = "regulator comment",
                RegistrationYear = DateTime.Now.Year
            };

        protected static PaymentDetailsViewModel GenerateValidPaymentDetailsViewModel() => new PaymentDetailsViewModel
        {
            OfflinePayment = "200.45"
        };

        protected static PaymentDetailsViewModel GenerateInvalidPaymentDetailsViewModel() => new PaymentDetailsViewModel
        {
            OfflinePayment = "200.45"
        };

        protected static void MergeFilterChoices(RegulatorRegistrationSubmissionSession session, RegistrationSubmissionsFilterModel newChoices)
        {
            session.LatestFilterChoices ??= new RegistrationSubmissionsFilterModel();

            session.LatestFilterChoices.OrganisationName = newChoices.OrganisationName ?? session.LatestFilterChoices.OrganisationName;
            session.LatestFilterChoices.OrganisationType = newChoices.OrganisationType ?? session.LatestFilterChoices.OrganisationType;
            session.LatestFilterChoices.RelevantYears = newChoices.RelevantYears ?? session.LatestFilterChoices.RelevantYears;
            session.LatestFilterChoices.PageNumber = newChoices.PageNumber > 0 ? newChoices.PageNumber : session.LatestFilterChoices.PageNumber;
            session.LatestFilterChoices.PageSize = newChoices.PageSize > 0 ? newChoices.PageSize : session.LatestFilterChoices.PageSize;
            session.LatestFilterChoices.Statuses = newChoices.Statuses ?? session.LatestFilterChoices.Statuses;
        }
    }
}
