namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.Registrations;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Sessions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

    [TestClass]
    public class RegistrationSubmissionsControllerTests : RegistrationSubmissionsTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        #region RegistrationSubmissions

        #region Initialisation and Basic Session state
        [TestMethod]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel()
        {
            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel();
            string expectedBackLink = "/regulators/home";

            // Act
            var result = await _controller.RegistrationSubmissions(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult.Model.GetType());

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_FromSession_When_Supplied_Null()
        {
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 2;
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(2);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnsModel_WithPageNumber_1_When_Supplied_Null()
        {
            var result = await _controller.RegistrationSubmissions(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(1);
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber.Should().Be(1);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_3()
        {
            var result = await _controller.RegistrationSubmissions(3);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var resultModel = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(3);
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber.Should().Be(3);
        }
        #endregion Initialisation and Basic Session state

        #region Session Models and Filter states between gets and posts
        #region Happy Path
        [TestMethod]
        public async Task RegistrationSubmissions_ShouldHandleNullSession()
        {
            // Arrange
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync((JourneySession)null);
            int pageNumber = 1;

            // Act
            var result = await _controller.RegistrationSubmissions(pageNumber);

            // Assert
            _mockSessionManager.Verify(sm => sm.GetSessionAsync(It.IsAny<ISession>()), Times.Once);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as RegistrationSubmissionsViewModel;
            Assert.IsNotNull(model);

            // Since session was null, it should use default page number logic
            Assert.AreEqual(pageNumber, model.ListViewModel.PaginationNavigationModel.CurrentPage);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_ShouldCreateANewJourneySession_WhenSessionManagerNull()
        {
            // Arrange
            var sut = new RegistrationSubmissionsController(
                null,
                null,
                _loggerMock.Object,
                _mockConfiguration.Object,
                _mockUrlsOptions.Object
                );

            // Act
            var result = sut.SessionManager;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JourneySessionManager));

            sut.Dispose();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Initialises_ListViewModel_When_New()
        {
            // Arrange
            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYear = "2025",
                Page = 1,
                PageSize = 500,
                SubmissionStatus = "pending"
            };

            SetupJourneySession(latestFilterChoices, null);

            // Act
            var result = await _controller.RegistrationSubmissions(null);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.Should().NotBeNull();
            session.CurrentPageNumber.Should().Be(1);
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYear.Should().Be("2025");
            session.LatestFilterChoices.Page.Should().Be(1);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.SubmissionStatus.Should().Be("pending");

            var model = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(1);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(1);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_ListViewModel_When_New_Page_Requested()
        {
            // Arrange
            int new_page_number = 4;

            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYear = "2025",
                Page = 2,
                PageSize = 500,
                SubmissionStatus = "pending"
            };

            SetupJourneySession(latestFilterChoices, null, 2);

            // Act
            var result = await _controller.RegistrationSubmissions(new_page_number);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.Should().NotBeNull();
            session.CurrentPageNumber.Should().Be(new_page_number);
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYear.Should().Be("2025");
            session.LatestFilterChoices.Page.Should().Be(new_page_number);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.SubmissionStatus.Should().Be("pending");

            var model = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(new_page_number);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(new_page_number);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Retrieves_ListViewModel_And_Adjusts_Filter_And_Model_After_Postback()
        {
            int expected_page_number = 2;

            RegistrationSubmissionsFilterModel emptyFilterChoices = new()
            { };

            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYear = "2025",
                Page = expected_page_number,
                PageSize = 500,
                SubmissionStatus = "pending"
            };

            SetupJourneySession(emptyFilterChoices, null);

            // Act
            var get_result = await _controller.RegistrationSubmissions(null);

            // Assert
            get_result.Should().BeOfType<ViewResult>();
            var model = (get_result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();


            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.LatestFilterChoices.OrganisationName.Should().BeNullOrEmpty();
            session.LatestFilterChoices.OrganisationType.Should().BeNullOrEmpty();
            session.LatestFilterChoices.RelevantYear.Should().BeNullOrEmpty();
            session.LatestFilterChoices.Page.Should().Be(1);
            session.LatestFilterChoices.SubmissionStatus.Should().BeNullOrEmpty();

            session.Should().NotBeNull();
            session.CurrentPageNumber.Should().Be(1);

            // Act again
            var result = await _controller.RegistrationSubmissions(latestFilterChoices, FilterActions.SubmitFilters);

            result.Should().BeOfType(typeof(RedirectToActionResult));
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.RegistrationSubmissionsAction);

            // Act Again

            get_result = await _controller.RegistrationSubmissions(4);

            // Test final session and viewmodel state
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYear.Should().Be("2025");
            session.LatestFilterChoices.Page.Should().Be(4);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.SubmissionStatus.Should().Be("pending");

            model = (get_result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(4);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(4);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_Clears_ListViewModel_And_Session_Filters_When_ClearFilters_Recevied()
        {
            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYear = "2025",
                Page = 2,
                PageSize = 200,
                SubmissionStatus = "pending"
            };

            SetupJourneySession(latestFilterChoices, null, 2);

            // Act
            var get_result = await _controller.RegistrationSubmissions(null);

            // Assert
            get_result.Should().BeOfType<ViewResult>();
            var model = (get_result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(2);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(2);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();

            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYear.Should().Be("2025");
            session.LatestFilterChoices.Page.Should().Be(2);
            session.LatestFilterChoices.PageSize.Should().Be(200);
            session.LatestFilterChoices.SubmissionStatus.Should().Be("pending");


            // Act again
            var result = await _controller.RegistrationSubmissions(null, FilterActions.ClearFilters);

            result.Should().BeOfType(typeof(RedirectToActionResult));
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.RegistrationSubmissionsAction);

            // Test final session and viewmodel state

            session.LatestFilterChoices.OrganisationName.Should().BeNullOrEmpty();
            session.LatestFilterChoices.OrganisationType.Should().BeNullOrEmpty();
            session.LatestFilterChoices.RelevantYear.Should().BeNullOrEmpty();
            session.LatestFilterChoices.Page.Should().Be(1);
            session.LatestFilterChoices.SubmissionStatus.Should().BeNullOrEmpty();
            model.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
        }
        #endregion Happy Path

        #region Sad Path
        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsEmpty()
        {
            var viewModel = new RegistrationSubmissionsFilterViewModel();
            var result = await _controller.RegistrationSubmissions(viewModel, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsInvalid()
        {
            var viewModel = new RegistrationSubmissionsFilterViewModel();
            var result = await _controller.RegistrationSubmissions(viewModel, "anything");
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_FilterAction_IsSubmitFilter_And_No_Filters_Supplied()
        {
            var result = await _controller.RegistrationSubmissions(null, FilterActions.SubmitFilters);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Return_PageNot_Found_When_No_Filter_Or_Query_Supplied()
        {
            var result = await _controller.RegistrationSubmissions(null, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task PostingTo_RegistrationSubmissions_Return_ErrorPage_When_Exception_Received()
        {
            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(null, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.Error);
        }

        [TestMethod]
        public async Task GettingFrom_RegistrationSubmissions_Return_ErrorPage_When_Exception_Received()
        {
            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(1);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.Error);
        }

        [TestMethod]
        public async Task PostTo_RegistrationSubmissions_Logs_Error_When_Exception_Received()
        {
            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(null, null);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.Error);
            _loggerMock.Verify(
                        x => x.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.Is<EventId>((eid) => eid == 1001),
                            It.IsAny<It.IsAnyType>(),
                            It.Is<Exception>((v, t) => v.ToString().Contains("Test")),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
        }

        [TestMethod]
        public async Task GettingFrom_RegistrationSubmissions_Logs_Error_When_Exception_Received()
        {
            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).Throws(new Exception("Test"));
            var result = await _controller.RegistrationSubmissions(1);
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            (result as RedirectToActionResult).ActionName.Should().Be(PagePath.Error);
            _loggerMock.Verify(
                        x => x.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while processing a message: Exception received processing GET to RegistrationSubmissionsController.RegistrationSubmissions")),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
        }
        #endregion Sad Path

        #endregion Session Models and Filter states between gets and posts
        #endregion RegistrationSubmissions

        #region GrantRegistrationSubmission

        [TestMethod]
        public async Task GrantRegistrationSubmission_SessionDataError_ReturnsPageNotFound()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.GrantRegistrationSubmission(Guid.NewGuid());

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange 
            var organisationId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{organisationId}";
            var mockUrlHelper = CreateUrlHelper(organisationId, locationUrl);
            var detailsModel = GenerateTestSubmissionDetailsViewModel(organisationId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.GrantRegistrationSubmission(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.GrantRegistrationSubmission), result.ViewName, "The view name should match the action name.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Get_Should_ReturnTo_CorrectViewWithModel()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(organisationId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{organisationId}";
            var mockUrlHelper = CreateUrlHelper(organisationId, locationUrl);
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.GrantRegistrationSubmission(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.GrantRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            result.Model.Should().BeOfType<GrantRegistrationSubmissionViewModel>();
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_Display_ErrorMessage_When_Nothing_Is_Selected()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(organisationId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;
            _controller.ModelState.AddModelError("IsGrantRegistrationConfirmed", "Select yes if you want to grant this registration");

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel { OrganisationId = organisationId }) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.GrantRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            var model = result.Model.As<GrantRegistrationSubmissionViewModel>();
            model.Should().NotBeNull();
            model.IsGrantRegistrationConfirmed.Should().BeNull();

            // Verify that ModelState has errors
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0, "ModelState should contain errors.");
            Assert.AreEqual("Select yes if you want to grant this registration", _controller.ModelState["IsGrantRegistrationConfirmed"].Errors[0].ErrorMessage, "The error message should match the expected message.");
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_RedirectTo_SubmissionDetails_When_GrantRegistrationIsNotConfirmed()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(organisationId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { OrganisationId = organisationId, IsGrantRegistrationConfirmed = false }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(organisationId);
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_Call_Facade_And_RedirectTo_SubmissionDetails_When_GrantRegistrationIsConfirmed()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(organisationId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { OrganisationId = organisationId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(organisationId);
            _facadeServiceMock.Verify(r => r.SubmitRegistrationDecision(It.IsAny<RegulatorRegistrationDecisionCreateRequest>()), Times.AtMostOnce);
        }

        #endregion GrantRegistrationSubmission

        #region QueryRegistrationSubmission

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_SessionDataError_ReturnsPageNotFOund()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = Guid.NewGuid(),
                Query = "query provided"
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(expectedViewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_SessionDataError_ReturnsPageNotFOund()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(Guid.NewGuid());

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange 
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                Query = "query provided"
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(id) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName, "The view name should match the action name.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_SetsCorrectBackLinkInViewData()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                Query = "query provided"
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(expectedViewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.RegistrationSubmissionsAction, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id
            };

            // Simulate an error in ModelState
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify that ModelState has errors
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0, "ModelState should contain errors.");
            Assert.AreEqual("Model state is invalid", _controller.ModelState["TestError"].Errors[0].ErrorMessage, "The error message should match the expected message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenQueryIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                Query = "Valid query within 400 characters." // Valid input
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissionsAction, result.ActionName); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                Query = new string('A', 401) // Exceeds 400 character limit
            };

            // Simulate model state error for the Query property
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError(nameof(model.Query), "Reason for querying application must be 400 characters or less");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for Query property
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)].Errors.Count > 0, "ModelState should contain an error for the Query property.");
            Assert.AreEqual("Reason for querying application must be 400 characters or less",
                _controller.ModelState[nameof(model.Query)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, locationUrl, "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsViewWithErrors_WhenNoQueryIsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new QueryRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                Query = null // No query provided
            };

            // Simulate the required field validation error for the Query property
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError(nameof(model.Query), "Enter the reason you are querying this registration application");

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.QueryRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for Query property
            Assert.IsTrue(_controller.ModelState[nameof(model.Query)].Errors.Count > 0, "ModelState should contain an error for the Query property.");
            Assert.AreEqual("Enter the reason you are querying this registration application",
                _controller.ModelState[nameof(model.Query)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        #endregion

        #region RejectRegistrationSubmission

        [TestMethod]
        public async Task RejectRegistrationSubmission_SessionDataError_ReturnsPageNotFOund()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(Guid.NewGuid());

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }
        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_SessionDataError_ReturnsPageNotFOund()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = Guid.NewGuid()
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(expectedViewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_ReturnsView_WithCorrectModel()
        {
            // Arrange 
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = id
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(RejectRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as RejectRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.RejectReason, resultViewModel.RejectReason);
            Assert.AreEqual(id, resultViewModel.OrganisationId);
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_ShouldSetCorrectBackLink()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(id);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Extract the GUID part using indexing and check for validity
            string[] segments = backLink.Split('/');
            Assert.IsNotNull(segments, "The back link should contain URL segments.");
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = id
            };

            // Simulate an error in ModelState
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify that ModelState has errors
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0, "ModelState should contain errors.");
            Assert.AreEqual("Model state is invalid", _controller.ModelState["TestError"].Errors[0].ErrorMessage, "The error message should match the expected message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenRejectionReasonIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                RejectReason = "Valid rejection reason within 400 characters." // Valid input
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(model) as RedirectResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is not null
            Assert.AreEqual(PagePath.RegistrationSubmissionsRoute, result.Url); // Ensure the user is redirected to the correct URL
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                RejectReason = new string('A', 401) // Exceeds 400 character limit
            };

            // Simulate model state error for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Reason for rejecting application must be 400 characters or less");

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for RejectReason property
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)].Errors.Count > 0, "ModelState should contain an error for the RejectReason property.");
            Assert.AreEqual("Reason for rejecting application must be 400 characters or less",
                _controller.ModelState[nameof(model.RejectReason)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenNoRejectionReasonIsProvided()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                OrganisationId = id,
                RejectReason = null // No rejection reason provided
            };

            // Simulate required field validation error for the RejectReason property
            _controller.ModelState.AddModelError(nameof(model.RejectReason), "Enter the reason you are rejecting this registration application");

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for RejectReason property
            Assert.IsTrue(_controller.ModelState[nameof(model.RejectReason)].Errors.Count > 0, "ModelState should contain an error for the RejectReason property.");
            Assert.AreEqual("Enter the reason you are rejecting this registration application",
                _controller.ModelState[nameof(model.RejectReason)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
        }

        #endregion

        #region RegistrationSubmissionDetails

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsViewResult_When_Receiving_Valid_Organisation_Id()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(organisationId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsPageNotFound_When_Receiving_InValid_Organisation_Id()
        {
            // Arrange
            var organisationId = Guid.NewGuid();

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsCorrectViewModel_ForValidOrganisationId()
        {
            // Arrange
            var organisationId = Guid.NewGuid(); // Simulate a valid Organisation ID
            string expectedViewName = nameof(_controller.RegistrationSubmissionDetails);
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(organisationId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedViewName, result.ViewName);
            var model = result.Model as RegistrationSubmissionDetailsViewModel;
            Assert.IsNotNull(model);

            // Assert model properties
            Assert.AreEqual(expectedViewModel.OrganisationId, model.OrganisationId);
            Assert.AreEqual(expectedViewModel.OrganisationReference, model.OrganisationReference);
            Assert.AreEqual(expectedViewModel.OrganisationName, model.OrganisationName);
            Assert.AreEqual(expectedViewModel.ApplicationReferenceNumber, model.ApplicationReferenceNumber);
            Assert.AreEqual(expectedViewModel.RegistrationReferenceNumber, model.RegistrationReferenceNumber);
            Assert.AreEqual(expectedViewModel.OrganisationType, model.OrganisationType);

            // Assert SubmissionDetailsViewModel properties
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Status, model.SubmissionDetails.Status);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.DecisionDate, model.SubmissionDetails.DecisionDate);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.TimeAndDateOfSubmission, model.SubmissionDetails.TimeAndDateOfSubmission);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedOnTime, model.SubmissionDetails.SubmittedOnTime);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedBy, model.SubmissionDetails.SubmittedBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.AccountRole, model.SubmissionDetails.AccountRole);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Telephone, model.SubmissionDetails.Telephone);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Email, model.SubmissionDetails.Email);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.DeclaredBy, model.SubmissionDetails.DeclaredBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Files.Count, model.SubmissionDetails.Files.Count);

            // Assert PaymentDetailsViewModel properties
            Assert.AreEqual(expectedViewModel.PaymentDetails.ApplicationProcessingFee, model.PaymentDetails.ApplicationProcessingFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.OnlineMarketplaceFee, model.PaymentDetails.OnlineMarketplaceFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.SubsidiaryFee, model.PaymentDetails.SubsidiaryFee);
            Assert.AreEqual(expectedViewModel.PaymentDetails.TotalChargeableItems, model.PaymentDetails.TotalChargeableItems);
            Assert.AreEqual(expectedViewModel.PaymentDetails.PreviousPaymentsReceived, model.PaymentDetails.PreviousPaymentsReceived);
            Assert.AreEqual(expectedViewModel.PaymentDetails.TotalOutstanding, model.PaymentDetails.TotalOutstanding);

            // Assert business address
            Assert.AreEqual(expectedViewModel.BusinessAddress.BuildingName, model.BusinessAddress.BuildingName);
            Assert.AreEqual(expectedViewModel.BusinessAddress.BuildingNumber, model.BusinessAddress.BuildingNumber);
            Assert.AreEqual(expectedViewModel.BusinessAddress.Street, model.BusinessAddress.Street);
            Assert.AreEqual(expectedViewModel.BusinessAddress.County, model.BusinessAddress.County);
            Assert.AreEqual(expectedViewModel.BusinessAddress.PostCode, model.BusinessAddress.PostCode);

            Assert.AreEqual(expectedViewModel.CompaniesHouseNumber, model.CompaniesHouseNumber);
            Assert.AreEqual(expectedViewModel.RegisteredNation, model.RegisteredNation);
            Assert.AreEqual(expectedViewModel.PowerBiLogin, model.PowerBiLogin);
            Assert.AreEqual(expectedViewModel.Status, model.Status);

            Assert.AreEqual(expectedViewModel.ProducerComments, model.ProducerComments);
            Assert.AreEqual(expectedViewModel.RegulatorComments, model.RegulatorComments);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_SetsCorrectBackLink()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(organisationId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(organisationId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissionsRoute}");
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_RedirectsToConfirmOfflinePaymentSubmission_WhenCalled_With_Valid_Model()
        {
            // Arrange
            var model = GenerateValidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());

            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(detailsModel);

            // Act
            var result = await _controller.SubmitOfflinePayment(model, detailsModel.OrganisationId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            _controller.ModelState.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_PresentsSubmissionDetailsView_WhenCalled_With_Invalid_Model()
        {
            // Arrange
            var model = GenerateInvalidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(detailsModel);

            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;

            _controller.ModelState.AddModelError("OfflinePayment", "The field is required.");

            // Act
            var result = await _controller.SubmitOfflinePayment(model, detailsModel.OrganisationId);
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            viewResult.ViewName.Should().Be("RegistrationSubmissionDetails");
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_ShouldHaveValidationErrors_WhenCalled_With_Invalid_Model()
        {
            // Arrange
            var model = GenerateInvalidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());

            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistration = detailsModel;
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(detailsModel);

            _controller.ModelState.AddModelError("OfflinePayment", "The field is required.");

            // Act
            var result = await _controller.SubmitOfflinePayment(model, detailsModel.OrganisationId);
            var viewResult = result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(viewResult);
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_RedirectsToPageNotFound_WhenCalled_With_OrganisationId_Mismatch()
        {
            // Arrange
            var model = GenerateValidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(detailsModel);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = detailsModel
            };

            // Act
            var result = await _controller.SubmitOfflinePayment(model, Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType(typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_RedirectsToPageNotFound_When_No_Details_In_Session()
        {
            // Arrange
            var model = GenerateValidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Returns(detailsModel);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession()
            {
                SelectedRegistration = null
            };

            // Act
            var result = await _controller.SubmitOfflinePayment(model, Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType(typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be(PagePath.PageNotFound);
        }
        #endregion

        #region Page Not Found
        [TestMethod]
        public async Task PageNotFound_SetsCurrentPageNumberToOne()
        {
            // Act
            await _controller.PageNotFound();

            // Assert
            Assert.AreEqual(1, _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber);
        }

        [TestMethod]
        public async Task PageNotFound_RedirectsToErrorPage()
        {
            // Act
            var result = await _controller.PageNotFound();

            // Assert
            Assert.IsInstanceOfType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual("Error", redirectResult.ControllerName);
            Assert.AreEqual(PagePath.Error, redirectResult.ActionName);
            Assert.AreEqual(404, redirectResult.RouteValues["statusCode"]);
            Assert.AreEqual(PagePath.RegistrationSubmissionsRoute, redirectResult.RouteValues["backLink"]);
        }
        #endregion Page Not Found

        #region ConfirmOfflinePaymentSubmission

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_NullOrganisationId_RedirectsToPageNotFound()
        {
            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission((Guid?)null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_ForAnInvalidOrganisationId()
        {
            // Arrange
            var invalidOrganisationId = Guid.NewGuid();
            SetupJourneySession(null, null); // No valid session with matching organisation

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(invalidOrganisationId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_ForAnEmptyOfflinePayment()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails.OfflinePayment = string.Empty; // Simulate empty offline payment
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(organisationId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_SetsCorrectBackLink()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails = GenerateValidPaymentDetailsViewModel();
            SetupJourneySession(null, submissionDetails);

            string expectedBackLink = "/expected/backlink/url";

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(organisationId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;

            // Check if backlink is correctly set in ViewData
            AssertBackLink(viewResult, expectedBackLink);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_ReturnsViewWithCorrectModel_ForAValidOrganisationIdAndOfflinePayment()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails = GenerateValidPaymentDetailsViewModel();
            SetupJourneySession(null, submissionDetails);

            var expectedViewModel = new ConfirmOfflinePaymentSubmissionViewModel
            {
                OrganisationId = organisationId,
                IsOfflinePaymentConfirmed = null,
                OfflinePaymentAmount = "10.00"
            };

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(organisationId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.AreEqual("ConfirmOfflinePaymentSubmission", viewResult.ViewName);

            var model = viewResult.Model as ConfirmOfflinePaymentSubmissionViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(expectedViewModel.OrganisationId, model.OrganisationId);
            Assert.AreEqual(submissionDetails.PaymentDetails.OfflinePayment, model.OfflinePaymentAmount);
            Assert.AreEqual(expectedViewModel.IsOfflinePaymentConfirmed, model.IsOfflinePaymentConfirmed);

            // Verify backlink setup
            AssertBackLink(viewResult, "/expected/backlink/url");
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToSubmissionDetails_ValidOrganisationIdAndValidModel()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails = GenerateValidPaymentDetailsViewModel();
            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                OrganisationId = organisationId,
                OfflinePaymentAmount = submissionDetails.PaymentDetails.OfflinePayment, // Valid amount
                IsOfflinePaymentConfirmed = true
            };

            // Set up session mock
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectResult));

            var redirectResult = result as RedirectResult;

            // Veryify the redirect URL
            string expectedRedirectUrl = _controller.Url.RouteUrl("SubmissionDetails", new { organisationId });
            Assert.AreEqual(expectedRedirectUrl, redirectResult.Url);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_WhenOfflinePaymentAmountIsEmpty()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails = new PaymentDetailsViewModel();
            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                OrganisationId = organisationId,
                OfflinePaymentAmount = submissionDetails.PaymentDetails.OfflinePayment, // Amount is null here
                IsOfflinePaymentConfirmed = false
            };

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(organisationId);
            submissionDetails.PaymentDetails = GenerateValidPaymentDetailsViewModel();
            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                OrganisationId = organisationId,
                OfflinePaymentAmount = submissionDetails.PaymentDetails.OfflinePayment
            };

            // Simulate an error in ModelState
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Set up session mock
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.ConfirmOfflinePaymentSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify that ModelState has errors
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0, "ModelState should contain errors.");
            Assert.AreEqual("Model state is invalid", _controller.ModelState["TestError"].Errors[0].ErrorMessage, "The error message should match the expected message.");
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_WhenOrgsanisationIdIsEmpty()
        {
            // Arrange
            var model = new ConfirmOfflinePaymentSubmissionViewModel();
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        #endregion

        private static Mock<IUrlHelper> CreateUrlHelper(Guid id, string locationUrl)
        {
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(locationUrl);
            return mockUrlHelper;
        }
    }
}