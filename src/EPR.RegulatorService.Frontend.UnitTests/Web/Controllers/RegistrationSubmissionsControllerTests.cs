namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using System;
    using System.Net;

    using EPR.Common.Authorization.Models;
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.Controllers.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.Sessions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Logging;

    using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

    [TestClass]
    public class RegistrationSubmissionsControllerTests : RegistrationSubmissionsTestBase
    {
        [TestInitialize]
        public void Setup() => SetupBase();

        #region RegistrationSubmissions

        #region Initialisation and Basic Session state
        [TestMethod]
        [DataRow(0, "")]
        [DataRow(1, "Environment Agency (EA)")]
        [DataRow(2, "Northern Ireland Environment Agency (NIEA)")]
        [DataRow(3, "Scottish Environment Protection Agency (SEPA)")]
        [DataRow(4, "Natural Resources Wales (NRW)")]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel_WhenShow2026RelevantYearFilter_IsDisabled(
            int nationId,
            string agencyName)
        {
            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel
            {
                AgencyName = agencyName
            };
            string expectedBackLink = "/regulators/home";

            // Ensure the Organisation has a valid NationId
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = nationId
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult!.Model.GetType());

            var resultModel = viewResult.Model as RegistrationSubmissionsViewModel;
            Assert.AreEqual(expectedViewModel.AgencyName, resultModel.AgencyName);
            Assert.IsFalse(resultModel.ListViewModel.RegistrationsFilterModel.Show2026RelevantYearFilter);

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        [DataRow(0, "")]
        [DataRow(1, "Environment Agency (EA)")]
        [DataRow(2, "Northern Ireland Environment Agency (NIEA)")]
        [DataRow(3, "Scottish Environment Protection Agency (SEPA)")]
        [DataRow(4, "Natural Resources Wales (NRW)")]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel_WhenShow2026RelevantYearFilter_IsEnabled(
            int nationId,
            string agencyName)
        {
            SetupBase(show2026RelevantYearFilter: true);

            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel
            {
                AgencyName = agencyName
            };
            string expectedBackLink = "/regulators/home";

            // Ensure the Organisation has a valid NationId
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = nationId
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult!.Model.GetType());

            var resultModel = viewResult.Model as RegistrationSubmissionsViewModel;
            Assert.AreEqual(expectedViewModel.AgencyName, resultModel.AgencyName);
            Assert.IsTrue(resultModel.ListViewModel.RegistrationsFilterModel.Show2026RelevantYearFilter);

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_ReturnsView_WithCorrectViewModel_WithNullNationId()
        {
            // Arrange
            var expectedViewModel = new RegistrationSubmissionsViewModel();
            string expectedBackLink = "/regulators/home";

            _journeySession.UserData.Organisations.Add(new Organisation());

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            Assert.IsInstanceOfType(expectedViewModel, viewResult!.Model.GetType());

            var actualBackLink = _controller.ViewBag.CustomBackLinkToDisplay;
            Assert.IsNotNull(actualBackLink);
            Assert.AreEqual(expectedBackLink, actualBackLink);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_FromSession_When_Supplied_Null()
        {
            // Arrange
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1
            });

            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber = 2;

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var resultModel = viewResult!.Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel!.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(2);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnsModel_WithPageNumber_1_When_Supplied_Null()
        {
            // Arrange
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var resultModel = viewResult!.Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel!.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(1);
            _journeySession.RegulatorRegistrationSubmissionSession.CurrentPageNumber.Should().Be(1);
        }

        [TestMethod]
        public async Task RegistrationsSubmissions_ReturnModel_WithPageNumber_3()
        {
            // Arrange
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(3);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var resultModel = viewResult!.Model as RegistrationSubmissionsViewModel;
            resultModel.Should().NotBeNull();
            resultModel!.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(3);
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

            // Setup a mock UserData object with Organisations and NationId
            var userData = new UserData
            {
                Organisations =
                [
                    new Organisation
                    {
                        NationId = 1
                    }
                ]
            };

            // Create the JourneySession and assign UserData to it
            var journeySession = new JourneySession
            {
                UserData = userData
            };

            // Inject the session with valid UserData
            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(journeySession);

            int pageNumber = 1;

            // Act
            var result = await _controller.RegistrationSubmissions(pageNumber);

            // Assert: Verify session retrieval was attempted once
            _mockSessionManager.Verify(sm => sm.GetSessionAsync(It.IsAny<ISession>()), Times.Once);

            // Assert: Verify the result is a ViewResult
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            // Assert: Verify the model is of the expected type
            var model = viewResult.Model as RegistrationSubmissionsViewModel;
            Assert.IsNotNull(model);

            // Since session was null initially, but we injected a session, it should use the page number logic
            Assert.AreEqual(pageNumber, model.ListViewModel.PaginationNavigationModel.CurrentPage);

            // Assert: Verify NationId handling
            Assert.IsNotNull(journeySession.UserData.Organisations.FirstOrDefault()?.NationId);
            Assert.AreEqual(1, journeySession.UserData.Organisations.FirstOrDefault()?.NationId);
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Initialises_ListViewModel_When_New()
        {
            // Arrange
            var existingSession = new RegulatorRegistrationSubmissionSession
            {
                CurrentPageNumber = 1, // Existing page number
                LatestFilterChoices = new RegistrationSubmissionsFilterModel()
            };

            _journeySession.RegulatorRegistrationSubmissionSession = existingSession;

            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1 // Ensure a valid NationId
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYears = "2025, 2026",
                PageNumber = 1,
                PageSize = 500,
                Statuses = "Pending"
            };

            // Apply new filter choices without overwriting the existing session.
            MergeFilterChoices(existingSession, latestFilterChoices);

            // Act
            var result = await _controller.RegistrationSubmissions(null);

            // Assert
            result.Should().BeOfType<ViewResult>();

            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.Should().NotBeNull();
            session.CurrentPageNumber.Should().Be(1);
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYears.Should().Be("2025, 2026");
            session.LatestFilterChoices.PageNumber.Should().Be(1);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.Statuses.Should().Be("Pending");

            var model = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model!.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(1);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(1);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2026Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Updates_ListViewModel_When_New_Page_Requested()
        {
            // Arrange
            int new_page_number = 4;

            // Setup the latest filter choices
            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYears = "2025",
                PageNumber = 2,
                PageSize = 500,
                Statuses = "Pending"
            };

            // Setup mock session with valid UserData
            var userData = new UserData
            {
                Organisations =
                [
                    new Organisation
                    {
                        NationId = 1
                    }
                ]
            };

            var journeySession = new JourneySession
            {
                UserData = userData,
                RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
                {
                    LatestFilterChoices = latestFilterChoices,
                    CurrentPageNumber = 2
                }
            };

            _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(journeySession);

            // Act
            var result = await _controller.RegistrationSubmissions(new_page_number);

            // Debugging: Check if the session page number was updated correctly
            var session = journeySession.RegulatorRegistrationSubmissionSession;
            Console.WriteLine($"Current Page Number in session after request: {session.CurrentPageNumber}");

            // Assert
            result.Should().BeOfType<ViewResult>();

            session.Should().NotBeNull();
            session.CurrentPageNumber.Should().Be(new_page_number, "The session page number should be updated to the new page number.");

            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYears.Should().Be("2025");
            session.LatestFilterChoices.PageNumber.Should().Be(new_page_number);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.Statuses.Should().Be("Pending");

            var model = (result as ViewResult).Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(new_page_number);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(new_page_number);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2026Checked.Should().BeFalse();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Retrieves_ListViewModel_And_Adjusts_Filter_And_Model_After_Postback()
        {
            // Arrange
            int expectedPageNumber = 2;

            RegistrationSubmissionsFilterModel emptyFilterChoices = new();

            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = "braun",
                OrganisationType = "small",
                RelevantYears = "2025, 2026",
                PageNumber = expectedPageNumber,
                PageSize = 500,
                Statuses = "Pending"
            };

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                LatestFilterChoices = emptyFilterChoices,
                CurrentPageNumber = 1
            };

            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1
            });

            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);

            // Act: Perform initial GET request
            var getResult = await _controller.RegistrationSubmissions(null);

            // Assert: Initial state
            getResult.Should().BeOfType<ViewResult>();
            var model = (getResult as ViewResult)?.Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();

            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.Should().NotBeNull();
            session.LatestFilterChoices.OrganisationName.Should().BeNullOrEmpty();
            session.LatestFilterChoices.OrganisationType.Should().BeNullOrEmpty();
            session.LatestFilterChoices.RelevantYears.Should().BeNullOrEmpty();
            session.LatestFilterChoices.PageNumber.Should().Be(1);
            session.LatestFilterChoices.Statuses.Should().BeNullOrEmpty();
            session.CurrentPageNumber.Should().Be(1);

            // Act: Simulate POST request with updated filters
            var postResult = await _controller.RegistrationSubmissions(latestFilterChoices, FilterActions.SubmitFilters);

            // Assert: POST redirects to GET action
            postResult.Should().BeOfType<RedirectToActionResult>();
            (postResult as RedirectToActionResult)?.ActionName.Should().Be(PagePath.RegistrationSubmissionsAction);

            // Act: Perform GET request with updated filters
            getResult = await _controller.RegistrationSubmissions(4);

            // Assert: Final state
            session.LatestFilterChoices.OrganisationName.Should().Be("braun");
            session.LatestFilterChoices.OrganisationType.Should().Be("small");
            session.LatestFilterChoices.RelevantYears.Should().Be("2025, 2026");
            session.LatestFilterChoices.PageNumber.Should().Be(4);
            session.LatestFilterChoices.PageSize.Should().Be(500);
            session.LatestFilterChoices.Statuses.Should().Be("Pending");

            model = (getResult as ViewResult)?.Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            model!.ListViewModel.PagedRegistrationSubmissions.Should().BeNull();
            model.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(4);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(4);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2026Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be("braun");
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationSubmissions_Clears_Filters_On_ClearFilters_Action()
        {
            // Arrange
            SetupSessionWithFilters("braun", "small", "2025, 2026", 2, 200, "Pending");
            // Act: Perform initial GET request
            var getResult = await _controller.RegistrationSubmissions(null);
            // Assert: Verify initial state
            AssertInitialFilters(getResult, "braun", "small", "2025, 2026", 2, 200, "Pending");
            // Act: Perform POST request to clear filters
            var postResult = await _controller.RegistrationSubmissions(null, FilterActions.ClearFilters);
            // Assert: Verify POST redirects to GET action
            AssertRedirectToGet(postResult);
            // Assert: Verify cleared state
            AssertClearedFilters();
        }
        private void SetupSessionWithFilters(
            string organisationName,
            string organisationType,
            string relevantYears,
            int pageNumber,
            int pageSize,
            string statuses)
        {
            RegistrationSubmissionsFilterModel latestFilterChoices = new()
            {
                OrganisationName = organisationName,
                OrganisationType = organisationType,
                RelevantYears = relevantYears,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Statuses = statuses
            };
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                LatestFilterChoices = latestFilterChoices,
                CurrentPageNumber = pageNumber
            };
            _journeySession.UserData.Organisations.Add(new Organisation
            {
                NationId = 1
            });
            _mockSessionManager.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(_journeySession);
        }
        private void AssertInitialFilters(
            object getResult,
            string organisationName,
            string organisationType,
            string relevantYears,
            int pageNumber,
            int pageSize,
            string statuses)
        {
            getResult.Should().BeOfType<ViewResult>();
            var model = (getResult as ViewResult)?.Model as RegistrationSubmissionsViewModel;
            model.Should().NotBeNull();
            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.Should().NotBeNull();
            model!.ListViewModel.PaginationNavigationModel.CurrentPage.Should().Be(pageNumber);
            model.ListViewModel.RegistrationsFilterModel.PageNumber.Should().Be(pageNumber);
            model.ListViewModel.RegistrationsFilterModel.IsStatusPendingChecked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2025Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.Is2026Checked.Should().BeTrue();
            model.ListViewModel.RegistrationsFilterModel.OrganisationName.Should().Be(organisationName);
            model.ListViewModel.RegistrationsFilterModel.IsOrganisationSmallChecked.Should().BeTrue();
            session.LatestFilterChoices.OrganisationName.Should().Be(organisationName);
            session.LatestFilterChoices.OrganisationType.Should().Be(organisationType);
            session.LatestFilterChoices.RelevantYears.Should().Be(relevantYears);
            session.LatestFilterChoices.PageNumber.Should().Be(pageNumber);
            session.LatestFilterChoices.PageSize.Should().Be(pageSize);
            session.LatestFilterChoices.Statuses.Should().Be(statuses);
        }
        private static void AssertRedirectToGet(object postResult)
        {
            postResult.Should().BeOfType<RedirectToActionResult>();
            (postResult as RedirectToActionResult)?.ActionName.Should().Be(PagePath.RegistrationSubmissionsAction);
        }
        private void AssertClearedFilters()
        {
            var session = _journeySession.RegulatorRegistrationSubmissionSession;
            session.LatestFilterChoices.OrganisationName.Should().BeNullOrEmpty();
            session.LatestFilterChoices.OrganisationType.Should().BeNullOrEmpty();
            session.LatestFilterChoices.RelevantYears.Should().BeNullOrEmpty();
            session.LatestFilterChoices.PageNumber.Should().Be(1);
            session.LatestFilterChoices.Statuses.Should().BeNullOrEmpty();
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
        public async Task GrantRegistrationSubmission_ReturnsPageNotFound_When_SubmissionId_DoesNot_Match_With_That_InSession()
        {
            // Act
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
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
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.GrantRegistrationSubmission(submissionId) as ViewResult;

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
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.GrantRegistrationSubmission(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.GrantRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            result.Model.Should().BeOfType<GrantRegistrationSubmissionViewModel>();
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_ReturnsPageNotFound_When_SubmissionId_Is_Invalid()
        {
            // Act
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel { SubmissionId = Guid.NewGuid() });

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_Display_ErrorMessage_When_Nothing_Is_Selected()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            _controller.ModelState.AddModelError("IsGrantRegistrationConfirmed", "Select yes if you want to grant this registration");

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel { SubmissionId = submissionId }) as ViewResult;

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
        public async Task GrantRegistrationSubmission_Post_Should_RedirectTo_SubmissionDetails_When_NotConfirmed()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = false }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(submissionId);
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_RedirectTo_ServiceNotAvailable_When_Facade_Returns_Fail()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Fail);

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("ServiceNotAvailable");
            result.RouteValues.First().Value.Should().Be($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");
            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_RedirectTo_ServiceNotAvailable_When_Facade_Throws()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, existingModel);

            var model = new GrantRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                IsGrantRegistrationConfirmed = true
            };

            var exception = new Exception("Test exception");

            // Set up the mock to throw an exception when SubmitRegulatorRegistrationDecisionAsync is called
            _facadeServiceMock
                .Setup(service => service.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.GrantRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Verify the back link in the route values is set correctly
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);

            // Verify that _logControllerError was called with correct parameters
            _loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received while granting submission")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        [DataRow(1, "Eng")]
        [DataRow(2, "NI")]
        [DataRow(3, "Sco")]
        [DataRow(4, "Wal")]
        [DataRow(5, "Eng")]
        public async Task GrantRegistrationSubmission_Post_RedirectTo_SubmissionDetails_OnSuccess(int nationId, string nationCode)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId, nationId, nationCode);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            }
            ;
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(submissionId);
            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        [DataRow(1, "Eng")]
        [DataRow(2, "NI")]
        [DataRow(3, "Sco")]
        [DataRow(4, "Wal")]
        public async Task On_GrantRegistrationSubmission_Update_Session_To_Remove_SubmissionDetails(int nationId, string nationCode)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId, nationId, nationCode);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            }
            ;
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            bool isSessionHoldsSubmissionId =
                _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations.TryGetValue(submissionId, out var selectedRegistration);

            Assert.IsNotNull(result);
            Assert.IsNull(selectedRegistration);
            Assert.IsFalse(isSessionHoldsSubmissionId);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(submissionId);
            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_Should_Not_Set_Vars_Needed_For_Reference_In_Resubmission()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId, 1, "Eng");
            detailsModel.IsResubmission = true;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success)
                .Callback<RegulatorDecisionRequest>((request) =>
                {
                    request.ExistingRegRefNumber.Should().NotBeNullOrWhiteSpace();
                    request.CountryName.Should().BeNullOrWhiteSpace();
                    request.RegistrationSubmissionType.Should().Be(RegistrationSubmissionType.NotSet);
                });

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true, IsResubmission = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            result.RouteValues.First().Value.Should().Be(submissionId);
            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        public async Task GrantRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new GrantRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId
            };

            // Simulate an error in ModelState
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.GrantRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.GrantRegistrationSubmission), result.ViewName, "The view name should match the action name.");
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

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        #endregion GrantRegistrationSubmission

        #region QueryRegistrationSubmission

        #region GET

        [TestMethod]
        public async Task QueryRegistrationSubmission_Get_SessionDataError_ReturnsPageNotFound()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(Guid.NewGuid());

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Get_ReturnsView_WithCorrectModel()
        {
            // Arrange 
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = id,
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
            var model = result.Model as QueryRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.SubmissionId, model.SubmissionId);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Get_SetsCorrectBackLinkInViewData()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.QueryRegistrationSubmission(id);

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

        #endregion GET

        #region POST

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_SessionDataError_ReturnsPageNotFound()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var expectedViewModel = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
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
        public async Task QueryRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = id
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
        public async Task QueryRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = id,
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
        [DataRow("Valid query within 400 characters")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("              valid query entry")]
        public async Task QueryRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_WhenQueryIsValid(string queryValue)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                Query = queryValue // Valid input
            };

            // Set up successful submission status
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectToActionResult;

            // Assert - Successful query and redirection
            Assert.IsNotNull(result);
            Assert.AreEqual(PagePath.RegistrationSubmissionsAction, result.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_ReturnsFailAndRedirectsCorrectly_WhenFacadeStatusReturnsFail()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                Query = "Valid query within 400 characters." // Valid input
            };

            // Set up an unsuccessful submission status
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Fail);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert - Unsuccessful query and redirection
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Assert route values
            string expectedUrl = $"{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            Assert.IsTrue(result.RouteValues.TryGetValue("backLink", out object backLink));
            Assert.AreEqual(expectedUrl, backLink);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_RedirectsToServiceNotAvailable_OnFacadeServiceException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                Query = "Valid reason"
            };

            // Set up the facade service to throw an exception
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(new Exception("Simulated facade exception"));

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert - Redirects to ServiceNotAvailable when an exception occurs
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task QueryRegistrationSubmission_Post_LogsErrorAndRedirectsToServiceNotAvailable_OnException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, existingModel);

            var model = new QueryRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                Query = "Test query"
            };

            var exception = new Exception("Test exception");

            // Set up the mock to throw an exception when SubmitRegulatorRegistrationDecisionAsync is called
            _facadeServiceMock
                .Setup(service => service.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.QueryRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Verify the back link in the route values is set correctly
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);

            // Verify that _logControllerError was called with correct parameters
            _loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received while querying submission")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion POST

        #endregion

        #region RejectRegistrationSubmission

        #region GET

        [TestMethod]
        public async Task RejectRegistrationSubmission_Get_SessionDataError_ReturnsPageNotFound()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(Guid.NewGuid());

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Get_ReturnsView_WithCorrectModel()
        {
            // Arrange 
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = id,
                IsResubmission = detailsModel.IsResubmission
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.RejectRegistrationSubmission), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(RejectRegistrationSubmissionViewModel));
            var resultViewModel = result.Model as RejectRegistrationSubmissionViewModel;
            Assert.AreEqual(expectedViewModel.SubmissionId, resultViewModel.SubmissionId);
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Get_ShouldSetCorrectBackLink()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
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

        #endregion GET

        #region POST

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_SessionDataError_ReturnsPageNotFound()
        {
            // Act
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var expectedViewModel = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                IsResubmission = detailsModel.IsResubmission
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.RejectRegistrationSubmission(expectedViewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL 
        }

        [TestMethod]
        public async Task RejectRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = id,
                IsResubmission = detailsModel.IsResubmission
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
        public async Task RejectRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var id = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{id}";

            var mockUrlHelper = CreateUrlHelper(id, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(id);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { id, detailsModel }
                }
            };
            var model = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = id,
                RejectReason = new string('A', 401), // Exceeds 400 character limit
                IsResubmission = detailsModel.IsResubmission
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
        [DataRow("Valid reject reason")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("             valid reason")]
        public async Task RejectRegistrationSubmission_Post_SavesRejectReasonInSessionObjectAndRedirectsCorrectly_ForAValidModelState(string rejectReason)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = rejectReason;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.Add(submissionId, detailsModel);

            var model = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsResubmission = detailsModel.IsResubmission
            };

            // Act
            var result = await _controller.RejectRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert - Successful save in session
            Assert.IsTrue(_journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[submissionId].RejectReason == rejectReason);
            bool isRegistrationSubmissionOrganisationDetailsChanged =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestRegistrationSubmissionOrganisationDetails);

            isRegistrationSubmissionOrganisationDetailsChanged.Should().BeTrue();
            latestRegistrationSubmissionOrganisationDetails.Should().NotBeNull();
            latestRegistrationSubmissionOrganisationDetails.RejectReason.Should().Be(rejectReason);
            latestRegistrationSubmissionOrganisationDetails.RegulatorComments.Should().Be(rejectReason.ToString());

            // Assert - Successful redirection
            Assert.IsNotNull(result);
            Assert.AreEqual("ConfirmRegistrationRefusal", result.RouteName);

            var routeValues = result.RouteValues.ToList();

            Assert.AreEqual("submissionId", routeValues[0].Key);
            Assert.AreEqual(detailsModel.SubmissionId, routeValues[0].Value);
        }

        [TestMethod]
        [DataRow("Valid reject reason")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("             valid reason")]
        public async Task RejectRegistrationSubmission_Post_SavesAndReturnsSuccessAndRedirectsCorrectly_ForValidReSubmissionRefusal(string rejectReason)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = rejectReason;
            detailsModel.IsResubmission = true;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.Add(submissionId, detailsModel);

            var model = new RejectRegistrationSubmissionViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsResubmission = detailsModel.IsResubmission
            };

            // Act
            // Set up successful submission status
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            var result = await _controller.RejectRegistrationSubmission(model) as RedirectToActionResult;

            // Assert - Successful refuse and redirection
            Assert.IsNotNull(result);
            Assert.AreEqual(PagePath.RegistrationSubmissionsAction, result.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);

            // Assert - Successful save in session
            Assert.IsTrue(_journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[submissionId].RejectReason == rejectReason);

            bool isRegistrationSubmissionOrganisationDetailsChanged =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestRegistrationSubmissionOrganisationDetails);

            isRegistrationSubmissionOrganisationDetailsChanged.Should().BeTrue();
            latestRegistrationSubmissionOrganisationDetails.Should().NotBeNull();
            latestRegistrationSubmissionOrganisationDetails.RejectReason.Should().Be(rejectReason);
            latestRegistrationSubmissionOrganisationDetails.RegulatorComments.Should().Be(rejectReason.ToString());
        }

        #endregion POST

        #endregion

        #region RegistrationSubmissionDetails

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsViewResult_When_Receiving_Valid_SubmissionId()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsPageNotFound_When_Receiving_InValid_SubmissionId()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsCorrectViewModel_ForValidSubmissionId()
        {
            // Arrange
            var submissionId = Guid.NewGuid(); // Simulate a valid submissionId
            string expectedViewName = nameof(_controller.RegistrationSubmissionDetails);
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedViewName, result.ViewName);
            var model = result.Model as RegistrationSubmissionDetailsViewModel;
            Assert.IsNotNull(model);

            // Assert model properties
            Assert.AreEqual(expectedViewModel.OrganisationId, model.OrganisationId);
            Assert.AreEqual(expectedViewModel.SubmissionId, model.SubmissionId);
            Assert.AreEqual(expectedViewModel.OrganisationReference, model.OrganisationReference);
            Assert.AreEqual(expectedViewModel.OrganisationName, model.OrganisationName);
            Assert.AreEqual(expectedViewModel.ReferenceNumber, model.ReferenceNumber);
            Assert.AreEqual(expectedViewModel.RegistrationReferenceNumber, model.RegistrationReferenceNumber);
            Assert.AreEqual(expectedViewModel.OrganisationType, model.OrganisationType);

            // Assert SubmissionDetailsViewModel properties
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Status, model.SubmissionDetails.Status);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.LatestDecisionDate, model.SubmissionDetails.LatestDecisionDate);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.TimeAndDateOfSubmission, model.SubmissionDetails.TimeAndDateOfSubmission);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedOnTime, model.SubmissionDetails.SubmittedOnTime);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.SubmittedBy, model.SubmissionDetails.SubmittedBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.AccountRole, model.SubmissionDetails.AccountRole);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Telephone, model.SubmissionDetails.Telephone);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Email, model.SubmissionDetails.Email);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.DeclaredBy, model.SubmissionDetails.DeclaredBy);
            Assert.AreEqual(expectedViewModel.SubmissionDetails.Files.Count, model.SubmissionDetails.Files.Count);

            //// Assert PaymentDetailsViewModel properties
            //Assert.AreEqual(expectedViewModel.PaymentDetails.ApplicationProcessingFee, model.PaymentDetails.ApplicationProcessingFee);
            //Assert.AreEqual(expectedViewModel.PaymentDetails.OnlineMarketplaceFee, model.PaymentDetails.OnlineMarketplaceFee);
            //Assert.AreEqual(expectedViewModel.PaymentDetails.SubsidiaryFee, model.PaymentDetails.SubsidiaryFee);
            //Assert.AreEqual(expectedViewModel.PaymentDetails.TotalChargeableItems, model.PaymentDetails.TotalChargeableItems);
            //Assert.AreEqual(expectedViewModel.PaymentDetails.PreviousPaymentsReceived, model.PaymentDetails.PreviousPaymentsReceived);
            //Assert.AreEqual(expectedViewModel.PaymentDetails.TotalOutstanding, model.PaymentDetails.TotalOutstanding);

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
            var submissionId = Guid.NewGuid();
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            // Check that the back link is correctly set in the ViewData
            AssertBackLink(result, $"/regulators/{PagePath.RegistrationSubmissionsRoute}");
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_Returns_Null_When_SelectedRegistration_And_SubmissionId_IsNull()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var expectedViewModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(expectedViewModel);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(null) as ViewResult;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_Post_RedirectsToConfirmOfflinePaymentSubmission_WhenCalled_With_Valid_Model()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var model = GenerateValidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(detailsModel);

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
            var submissionId = Guid.NewGuid();
            var model = GenerateInvalidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(detailsModel);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };

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
            var submissionId = Guid.NewGuid();
            var model = GenerateInvalidPaymentDetailsViewModel();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(detailsModel);
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(detailsModel);

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
        public async Task SubmitOfflinePayment_Post_RedirectsToPageNotFound_When_No_Details_In_Session()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var model = GenerateValidPaymentDetailsViewModel();
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(default(RegistrationSubmissionDetailsViewModel));

            // Act
            var result = await _controller.SubmitOfflinePayment(model, submissionId);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType(typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            redirect.ActionName.Should().Be(PagePath.PageNotFound);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsPageNotFound_When_HttpRequestException_Thrown_With_NotFound_Status()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Throws(exception);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual(PagePath.PageNotFound, redirect.ActionName);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsErrorPage_When_General_Exception_Thrown()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var exception = new Exception("General exception");

            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).Throws(exception);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual(PagePath.Error, redirect.ActionName);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsPageNotFound_When_HttpRequestException_Thrown_With_NotFound_StatusCode()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var httpRequestException = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);

            // Setup the _facadeServiceMock to throw the HttpRequestException
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ThrowsAsync(httpRequestException);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            // Verify that the logger was called with the expected error message
            _loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received processing GET to RegistrationSubmissionsController")),
                    It.Is<Exception>(ex => ex == httpRequestException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Check the redirection to "PageNotFound"
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(PagePath.PageNotFound, redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RegistrationSubmissionDetails_ReturnsErrorPage_When_HttpRequestException_Thrown_With_NonNotFound_StatusCode()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var httpRequestException = new HttpRequestException("Error", null, HttpStatusCode.InternalServerError); // Non-NotFound status code

            // Setup the _facadeServiceMock to throw the HttpRequestException
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ThrowsAsync(httpRequestException);

            // Act
            var result = await _controller.RegistrationSubmissionDetails(submissionId);

            // Assert
            // Check the redirection to "Error"
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(PagePath.Error, redirectResult.ActionName);
            Assert.AreEqual("Error", redirectResult.ControllerName);

            // Verify that the logger was called with the expected error message
            _loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received processing GET to RegistrationSubmissionsController")),
                    It.Is<Exception>(ex => ex == httpRequestException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
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
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_ForAnInvalidSubmissionId()
        {
            // Arrange
            var invalidSubmissionId = Guid.NewGuid();
            SetupJourneySession(null, null); // No valid session with matching submission Id

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(invalidSubmissionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_ForAnEmptyOfflinePayment(string offlinePayment)
        {
            // Arrange
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                { "OfflinePaymentAmount", offlinePayment }
            };
            _controller.TempData = tempData;
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(submissionId);

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
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                { "OfflinePaymentAmount", "200.45" }
            };
            _controller.TempData = tempData;
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, submissionDetails);

            string expectedBackLink = "/expected/backlink/url";

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(submissionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;

            // Check if backlink is correctly set in ViewData
            AssertBackLink(viewResult, expectedBackLink);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_ReturnsViewWithCorrectModel_ForAValidSubmissionIdAndOfflinePayment()
        {
            // Arrange
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            {
                { "OfflinePaymentAmount", "10.00" }
            };
            _controller.TempData = tempData;
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(submissionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.AreEqual("ConfirmOfflinePaymentSubmission", viewResult.ViewName);
            var model = viewResult.Model as ConfirmOfflinePaymentSubmissionViewModel;
            Assert.IsNotNull(model);
            model.SubmissionId.Should().Be(submissionId);
            model.IsOfflinePaymentConfirmed.Should().BeNull();
            model.OfflinePaymentAmount.Should().Be("10.00");

            // Verify backlink setup
            AssertBackLink(viewResult, "/expected/backlink/url");
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToSubmissionDetails_ValidSubmissionIdAndValidModel()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";
            SetupJourneySession(null, submissionDetails);
            _paymentFacadeServiceMock.Setup(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>())).ReturnsAsync(EndpointResponseStatus.Success);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                OfflinePaymentAmount = "200.45", // Valid amount
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
            string expectedRedirectUrl = _controller.Url.RouteUrl("SubmissionDetails", new { submissionId });
            Assert.AreEqual(expectedRedirectUrl, redirectResult.Url);
            _paymentFacadeServiceMock.Verify(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>()), Times.AtMostOnce);
            _facadeServiceMock.Verify(r => r.SubmitRegistrationFeePaymentAsync(It.IsAny<FeePaymentRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsTo_ServiceNotAvailable_When_Payment_Facade_Returns_NonSuccess()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";

            SetupJourneySession(null, submissionDetails);
            _paymentFacadeServiceMock.Setup(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>())).ReturnsAsync(EndpointResponseStatus.Fail);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                OfflinePaymentAmount = "200.45", // Valid amount
                IsOfflinePaymentConfirmed = true
            };

            // Set up session mock
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);

            // Veryify the redirect URL
            result.RouteName.Should().Be("ServiceNotAvailable");
            result.RouteValues.First().Value.Should().Be($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");
            _paymentFacadeServiceMock.Verify(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>()), Times.AtMostOnce);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_WhenOfflinePaymentAmountIsInvalid(string offlinePaymentAmount)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";

            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = offlinePaymentAmount
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
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";

            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                OfflinePaymentAmount = "200.45"
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
        public async Task ConfirmOfflinePaymentSubmission_Post_ReturnsToSubmissionDetails_WhenNotConfirmed()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";

            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                OfflinePaymentAmount = "200.45",
                IsOfflinePaymentConfirmed = false
            };

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("SubmissionDetails");
            var routeValues = result.RouteValues.First();
            routeValues.Key.Should().Be("SubmissionId");
            routeValues.Value.Should().Be(submissionId);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_RedirectsToPageNotFound_WhenSubmissionIdIsNull()
        {
            // Arrange
            var model = new ConfirmOfflinePaymentSubmissionViewModel { SubmissionId = Guid.NewGuid() };
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(Guid.NewGuid());
            SetupJourneySession(null, submissionDetails);
            _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionDetails(It.IsAny<Guid>())).ReturnsAsync(default(RegistrationSubmissionDetailsViewModel));

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
        public async Task ConfirmOfflinePaymentSubmission_Logs_And_RedirectsTo_ServiceNotAvailable_When_PaymentFacade_Fails()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            _controller.TempData["OfflinePaymentAmount"] = "200.45";

            SetupJourneySession(null, submissionDetails);
            _paymentFacadeServiceMock.Setup(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>())).ReturnsAsync(EndpointResponseStatus.Fail);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                SubmissionId = submissionId,
                OfflinePaymentAmount = "200.45",
                IsOfflinePaymentConfirmed = true
            };

            // Set up session mock
            SetupJourneySession(null, submissionDetails);

            // Act
            var result = await _controller.ConfirmOfflinePaymentSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("ServiceNotAvailable");
            result.RouteValues.First().Value.Should().Be($"{PagePath.RegistrationSubmissionDetails}/{submissionId}");
            _paymentFacadeServiceMock.Verify(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>()), Times.AtMostOnce);
            _facadeServiceMock.Verify(r => r.SubmitRegistrationFeePaymentAsync(It.IsAny<FeePaymentRequest>()), Times.Never);
        }

        #endregion

        #region CancellationConfirmation

        [TestMethod]
        public async Task CancellationConfirmation_RedirectsToPageNotFound_WhenOrgsanisationIdIsEmpty()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancellationConfirmation(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancellationConfirmation_RedirectsToPageNotFound_WhenSubmissionIdIsInvalid()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancellationConfirmation(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancellationConfirmation_RedirectsToPageNotFound_WhenOrgsanisationNameIsNull()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            existingModel.OrganisationName = null; // Organisation name is null

            SetupJourneySession(null, existingModel);

            // Act
            var result = await _controller.CancellationConfirmation(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancellationConfirmation_RedirectsToPageNotFound_WhenOrgsanisationNameIsEmpty()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);

            existingModel.OrganisationName = string.Empty; // Organisation name is empty

            SetupJourneySession(null, existingModel);

            // Act
            var result = await _controller.CancellationConfirmation(submissionId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancellationConfirmation_ReturnsViewWithModel_ForAValidSubmissionIdAndOrganisationName()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string organisationName = "Test Organisation";
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            existingModel.OrganisationName = organisationName;
            SetupJourneySession(null, existingModel);

            // Act
            var result = await _controller.CancellationConfirmation(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(nameof(_controller.CancellationConfirmation), result.ViewName);

            var model = result.Model as CancellationConfirmationViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(submissionId, model.SubmissionId);
            Assert.AreEqual(organisationName, model.OrganisationName);
        }

        #endregion

        #region CancelRegistrationSubmission

        #region GET

        [TestMethod]
        public async Task CancelRegistrationSubmission_Get_RedirectsToPageNotFound_WhenSubmissionIdIsEmpty()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancelRegistrationSubmission((Guid?)null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancelRegistrationSubmission_Get_RedirectsToPageNotFound_WhenSubmissionIdIsInvalid()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancelRegistrationSubmission(Guid.NewGuid());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancelRegistrationSubmission_Get_ReturnsViewWithModel_ForAValidSubmissionIdAndOrganisationName()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            SetupJourneySession(null, existingModel);

            // Act
            var result = await _controller.CancelRegistrationSubmission(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(nameof(_controller.CancelRegistrationSubmission), result.ViewName);

            var model = result.Model as CancelRegistrationSubmissionViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(submissionId, model.SubmissionId);
        }

        #endregion GET

        #region POST

        [TestMethod]
        public async Task CancelRegistrationSubmission_Post_ReturnsPageNotFound_ForErroneousSessionData()
        {
            // Act
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            var expectedViewModel = new CancelRegistrationSubmissionViewModel
            {
                SubmissionId = Guid.NewGuid(),
                CancellationReason = "reason provided"
            };

            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.CancelRegistrationSubmission(expectedViewModel);

            // Assert
            var viewResult = result as RedirectToActionResult;
            Assert.IsNotNull(viewResult, "Result should be of type ViewResult.");

            Assert.AreEqual(PagePath.PageNotFound, viewResult.ActionName); // Ensure the user is redirected to the correct URL

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelRegistrationSubmission_Post_ReturnsPageNotFound_ForAnInValidSubmissionId()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancelRegistrationSubmission(new CancelRegistrationSubmissionViewModel());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelRegistrationSubmission_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId
            };

            // Simulate an error in ModelState
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.CancelRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.CancelRegistrationSubmission), result.ViewName, "The view name should match the action name.");
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

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelRegistrationSubmission_Post_ReturnsExpectedError_WhenInputIsGreaterThan400()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };
            var model = new CancelRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationReason = new string('A', 401) // Exceeds 400 character limit
            };

            // Simulate model state error for the CancellationReason property
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError(nameof(model.CancellationReason), "Reason for cancelling application must be 400 characters or less");

            // Act
            var result = await _controller.CancelRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.CancelRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for CancellationReason property
            Assert.IsTrue(_controller.ModelState[nameof(model.CancellationReason)].Errors.Count > 0, "ModelState should contain an error for the CancellationReason property.");
            Assert.AreEqual("Reason for cancelling application must be 400 characters or less",
                _controller.ModelState[nameof(model.CancellationReason)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, locationUrl, "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        [DataRow("Valid cancellation reason")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("                Valid cancellation reason")]
        public async Task CancelRegistrationSubmission_Post_SavesCancellationReasonInSessionObjectAndRedirectsCorrectly_ForAValidModelState(
            string cancellationReason)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = cancellationReason;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelRegistrationSubmissionViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                CancellationReason = detailsModel.CancellationReason
            };

            // Act
            var result = await _controller.CancelRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert - Successful save in session
            Assert.IsTrue(_journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations[submissionId].CancellationReason == cancellationReason);

            // Assert - Successful redirection
            Assert.IsNotNull(result);
            Assert.AreEqual("CancelDateRegistrationSubmission", result.RouteName);

            var routeValues = result.RouteValues.ToList();

            Assert.AreEqual("submissionId", routeValues[0].Key);
            Assert.AreEqual(detailsModel.SubmissionId, routeValues[0].Value);
        }

        #endregion POST

        #endregion

        #region ConfirmRegistrationRefusal

        #region GET

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Get_RedirectsToPageNotFound_WhenSubmissionIdIsInvalid()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(submissionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Get_RedirectsToPageNotFound_WhenSubmissionIdIsNull()
        {
            // Act
            var result = await _controller.ConfirmRegistrationRefusal((Guid?)null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Get_RedirectsToPageNotFound_WhenExisitngModelIsNull()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(Guid.NewGuid());

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("PageNotFound", redirectResult.ActionName);
            Assert.AreEqual("RegistrationSubmissions", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Get_ReturnsView_WithCorrectViewModelAndBackLink()
        {
            // Arrange 
            var submissionId = Guid.NewGuid();
            string expectedRejectReason = "Valid reject reason";
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = expectedRejectReason;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var expectedViewModel = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason
            };

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.ConfirmRegistrationRefusal(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(nameof(_controller.ConfirmRegistrationRefusal), result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(ConfirmRegistrationRefusalViewModel));
            var resultViewModel = result.Model as ConfirmRegistrationRefusalViewModel;
            Assert.AreEqual(expectedViewModel.SubmissionId, resultViewModel.SubmissionId);
            Assert.AreEqual(expectedViewModel.RejectReason, resultViewModel.RejectReason);

            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Extract the GUID part using indexing and check for validity
            string[] segments = backLink.Split('/');
            Assert.IsNotNull(segments, "The back link should contain URL segments.");
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
            Assert.AreEqual(segments[3], detailsModel.SubmissionId.ToString(), "Back link should contain the correct Submission ID");
        }

        #endregion

        #region POST

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_RedirectsToPageNotFound_WhenSubmissionIdIsInvalid()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var viewModel = new ConfirmRegistrationRefusalViewModel { SubmissionId = submissionId };

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(viewModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_RedirectsToPageNotFound_WhenSubmissionIdIsNull()
        {
            // Arrange
            var viewModel = new ConfirmRegistrationRefusalViewModel { SubmissionId = null };

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(viewModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_ReturnsViewCorrectly_WhenModelStateIsInvalid()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);

            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = submissionId
            };

            // Simulate an error in ModelState
            _controller.ModelState.AddModelError("TestError", "Model state is invalid");

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.ConfirmRegistrationRefusal), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify that ModelState has errors
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0, "ModelState should contain errors.");
            Assert.AreEqual("Model state is invalid", _controller.ModelState["TestError"].Errors[0].ErrorMessage, "The error message should match the expected message.");

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_RedirectsToSubmissionDetails_WhenIsRegistrationRefusalConfirmedIsFalse()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionDetails = GenerateTestSubmissionDetailsViewModel(submissionId);
            submissionDetails.RejectReason = "Valid reason";

            // Set up session mock
            SetupJourneySession(null, submissionDetails);

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = submissionDetails.SubmissionId,
                RejectReason = submissionDetails.RejectReason,
                IsRegistrationRefusalConfirmed = false
            };

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(model);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));

            var redirectResult = result as RedirectToRouteResult;

            // Veryify the redirect URL
            redirectResult.RouteName.Should().Be("SubmissionDetails");
            redirectResult.RouteValues.First().Value.Should().Be(submissionId);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        [DataRow("Valid rejection reason")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("                Valid rejection reason")]
        public async Task ConfirmRegistrationRefusal_Post_ReturnsSuccessAndRedirectsCorrectly_ForValidSubmission(string rejectionReason)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.Status = Frontend.Core.Enums.RegistrationSubmissionStatus.Refused;
            detailsModel.RejectReason = rejectionReason;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsRegistrationRefusalConfirmed = true
            };

            // Act
            // Set up successful submission status
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            _controller.Url = mockUrlHelper.Object;

            var result = await _controller.ConfirmRegistrationRefusal(model) as RedirectToActionResult;

            // Assert - Successful query and redirection
            Assert.IsNotNull(result);
            Assert.AreEqual(PagePath.RegistrationSubmissionsAction, result.ActionName);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_ReturnsFailAndRedirectsCorrectly_WhenFacadeStatusReturnsFail()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = "Valid reason.";
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsRegistrationRefusalConfirmed = true
            };

            // Set up an unsuccessful call
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Fail);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(model) as RedirectToRouteResult;

            // Assert - Unsuccessful reject and redirection
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Assert route values
            string expectedUrl = $"{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            Assert.IsTrue(result.RouteValues.TryGetValue("backLink", out object backLink));
            Assert.AreEqual(expectedUrl, backLink);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_RedirectsToServiceNotAvailable_OnFacadeServiceException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = "Valid reason";
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsRegistrationRefusalConfirmed = true
            };

            // Set up the facade service to throw an exception
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(new Exception("Simulated facade exception"));

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.ConfirmRegistrationRefusal(model) as RedirectToRouteResult;

            // Assert - Redirects to ServiceNotAvailable when an exception occurs
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task ConfirmRegistrationRefusal_Post_ReturnsExpectedError_WhenSubmittingWithoutSelection()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.RejectReason = "Valid reason";
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new ConfirmRegistrationRefusalViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                RejectReason = detailsModel.RejectReason,
                IsRegistrationRefusalConfirmed = null // Deliberately setting selection bool as null
            };

            // Simulate model state error for the IsRegistrationRefusalConfirmed property
            _controller.ModelState.AddModelError(nameof(model.IsRegistrationRefusalConfirmed), "Select yes if you want to refuse this registration application");

            // Act
            _controller.Url = mockUrlHelper.Object;
            var result = await _controller.ConfirmRegistrationRefusal(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.ConfirmRegistrationRefusal), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for IsRegistrationRefusalConfirmed property
            Assert.IsTrue(_controller.ModelState[nameof(model.IsRegistrationRefusalConfirmed)].Errors.Count > 0, "ModelState should contain an error for the IsRegistrationRefusalConfirmed property.");
            Assert.AreEqual("Select yes if you want to refuse this registration application",
                _controller.ModelState[nameof(model.IsRegistrationRefusalConfirmed)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");
            Assert.AreEqual(segments[3], detailsModel.SubmissionId.ToString(), "Back link should contain the correct Submission ID");

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        #endregion

        #endregion

        #region CancelDateRegistrationSubmission

        #region GET

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_RedirectsToPageNotFound_WhenSubmissionIdIsEmpty()
        {
            // Arrange
            SetupJourneySession(null, null);

            // Act
            var result = await _controller.CancelDateRegistrationSubmission((Guid?)null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectToActionResult = result as RedirectToActionResult;

            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Should_Call_CancelDateRegistrationSubmissionView()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            SetupJourneySession(null, null);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(submissionId) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result.ViewName.Should().Be("CancelDateRegistrationSubmission");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.CancelRegistrationSubmission}/", "Back link should start with the expected URL.");
        }

        #endregion

        #region POST

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Post_RedirectsToPageNotFound_When_There_Is_No_SessionData()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationDate = null,
                Day = null,
                Month = null,
                Year = null
            };
            // Simulate the required field validation error for the CancellationReason property
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError(nameof(model.CancellationDate), "Date of cancellation must be a real Date");
            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectToActionResult = result as RedirectToActionResult;
            // Veryify the correct redirect
            Assert.AreEqual("RegistrationSubmissions", redirectToActionResult.ControllerName);
            Assert.AreEqual("PageNotFound", redirectToActionResult.ActionName);
        }

        [TestMethod]
        [DataRow("Valid cancellation reason")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("                Valid cancellation reason")]
        public async Task CancelDateRegistrationSubmission_Post_ReturnsSuccessAndRedirectsCorrectly_ForValidSubmission(
            string cancellationReason)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = SetupJourneySessionForCancellation(submissionId, cancellationReason);

            var expectedDate = new DateTime(2025, 4, 3, 0, 0, 0, DateTimeKind.Utc);

            var model = CreateCancelDateSubmissionViewModel(submissionId, expectedDate);

            // Set up successful cancellation submission
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            result.RouteName.Should().Be("CancellationConfirmation");
            result.RouteValues.First().Value.Should().Be(detailsModel.SubmissionId);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        private RegistrationSubmissionDetailsViewModel SetupJourneySessionForCancellation(
            Guid submissionId,
            string cancellationReason)
        {
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = cancellationReason;
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            return detailsModel;
        }

        private static CancelDateRegistrationSubmissionViewModel CreateCancelDateSubmissionViewModel(
            Guid submissionId,
            DateTime expectedDate) => new()
            {
                SubmissionId = submissionId,
                CancellationDate = expectedDate,
                Day = expectedDate.Day,
                Month = expectedDate.Month,
                Year = expectedDate.Year
            };

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Post_ReturnsViewWithErrors_WhenNoCancellationDateIsProvided()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationDate = null,
                Day = null,
                Month = null,
                Year = null

            };

            // Simulate the required field validation error for the CancellationReason property
            _controller.Url = mockUrlHelper.Object;
            _controller.ModelState.AddModelError(nameof(model.CancellationDate), "Date of cancellation must be a real Date");

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult when ModelState is invalid.");
            Assert.AreEqual(nameof(_controller.CancelDateRegistrationSubmission), result.ViewName, "The view name should match the action name.");
            Assert.AreEqual(model, result.Model, "The returned model should match the input model.");

            // Verify ModelState error for CancellationReason property
            Assert.IsTrue(_controller.ModelState[nameof(model.CancellationDate)].Errors.Count > 0, "ModelState should contain an error for the CancellationReason property.");
            Assert.AreEqual("Date of cancellation must be a real Date",
                _controller.ModelState[nameof(model.CancellationDate)].Errors[0].ErrorMessage, "The error message should match the expected validation message.");

            // Verify that a back link is set with the expected format, including a GUID
            string backLink = _controller.ViewData["BackLinkToDisplay"] as string;
            Assert.IsNotNull(backLink, "BackLinkToDisplay should be set in ViewData.");
            StringAssert.StartsWith(backLink, $"/regulators/{PagePath.RegistrationSubmissionDetails}/", "Back link should start with the expected URL.");

            // Check that the back link contains a valid GUID at the end
            string[] segments = backLink.Split('/');
            Assert.IsTrue(Guid.TryParse(segments[^1], out _), "Back link should contain a valid GUID.");

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Post_ReturnsFailAndRedirectsCorrectly_WhenFacadeStatusReturnsFail()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = "Valid cancellation reason";

            var expectedDate = new DateTime(2025, 4, 3, 0, 0, 0, DateTimeKind.Utc);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                CancellationDate = expectedDate,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            // Set up an unsuccessful cancellation submission status
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Fail);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Assert route values
            string expectedUrl = $"{PagePath.RegistrationSubmissionDetails}/{submissionId}";
            Assert.IsTrue(result.RouteValues.TryGetValue("backLink", out object backLink));
            Assert.AreEqual(expectedUrl, backLink);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Post_RedirectsToServiceNotAvailable_OnFacadeServiceException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = "Valid cancellation reason";

            var expectedDate = new DateTime(2025, 4, 3, 0, 0, 0, DateTimeKind.Utc);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = detailsModel.SubmissionId,
                CancellationDate = expectedDate,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            // Set up the facade service to throw an exception
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(new Exception("Simulated facade exception"));

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert - Redirects to ServiceNotAvailable when an exception occurs
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);
        }

        [TestMethod]
        public async Task CancelDateRegistrationSubmission_Post_LogsErrorAndRedirectsToServiceNotAvailable_OnException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var existingModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            existingModel.CancellationReason = "Valid cancellation reason";
            var expectedDate = new DateTime(2025, 4, 3, 0, 0, 0, DateTimeKind.Utc);

            SetupJourneySession(null, existingModel);

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = existingModel.SubmissionId,
                CancellationDate = expectedDate,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            var exception = new Exception("Test exception");

            // Set up the mock to throw an exception when SubmitRegulatorRegistrationDecisionAsync is called
            _facadeServiceMock
                .Setup(service => service.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ServiceNotAvailable", result.RouteName);

            // Verify the back link in the route values is set correctly
            Assert.AreEqual($"{PagePath.RegistrationSubmissionDetails}/{submissionId}", result.RouteValues["backLink"]);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);

            // Verify that _logControllerError was called with correct parameters
            _loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Exception received while cancelling submission")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #endregion CancelDateRegistrationSubmission

        #region OrganisationDetailsChangeHistorySession

        [TestMethod]
        public async Task When_CancelDateRegistrationSubmission_Post_Success_Then_Should_Update_OrganisationDetailsChangeHistory_InSession()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = "Valid cancellation reason";
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationDate = DateTime.Now,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            // Set up successful cancellation submission
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            Assert.IsNotNull(result);

            bool upadtedChangeHistory =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestOrganisationDetails);

            upadtedChangeHistory.Should().BeTrue();
            latestOrganisationDetails.RegulatorComments.Should().BeSameAs(detailsModel.CancellationReason);
            latestOrganisationDetails.SubmissionStatus.Should().Be(RegistrationSubmissionStatus.Cancelled);
        }

        [TestMethod]
        public async Task Fetch_RegistrationSubmissionDetailsViewModel_From_OrganisationDetailsChangeHistory_InSession()
        {
            // Arrange
            var submissionId = Guid.NewGuid();

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = "Valid cancellation reason";

            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                OrganisationDetailsChangeHistory = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>
                                                    {
                                                        { submissionId, detailsModel }
                                                    }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationDate = DateTime.Now,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            // Set up successful cancellation submission
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            Assert.IsNotNull(result);

            bool upadtedChangeHistory =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestOrganisationDetails);

            upadtedChangeHistory.Should().BeTrue();
            latestOrganisationDetails.RegulatorComments.Should().BeSameAs(detailsModel.CancellationReason);
            latestOrganisationDetails.SubmissionStatus.Should().Be(RegistrationSubmissionStatus.Cancelled);
        }

        [TestMethod]
        public async Task When_CancelDateRegistrationSubmission_Post_Fail_Then_Should_Not_Update_OrganisationDetailsChangeHistory_InSession()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string locationUrl = $"/regulators/{PagePath.RegistrationSubmissionDetails}/{submissionId}";

            var mockUrlHelper = CreateUrlHelper(submissionId, locationUrl);

            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            detailsModel.CancellationReason = "Valid cancellation reason";

            var expectedDate = new DateTime(2025, 4, 3, 0, 0, 0, DateTimeKind.Utc);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var model = new CancelDateRegistrationSubmissionViewModel
            {
                SubmissionId = submissionId,
                CancellationDate = expectedDate,
                Day = 3,
                Month = 4,
                Year = 2025
            };

            // Set up successful cancellation submission
            _facadeServiceMock
                .Setup(mock => mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Fail);

            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = await _controller.CancelDateRegistrationSubmission(model) as RedirectToRouteResult;

            Assert.IsNotNull(result);

            // Verify that the facade service was called the expected number of times
            _facadeServiceMock.Verify(mock =>
                mock.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.Once);

            bool upadtedChangeHistory =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestOrganisationDetails);

            upadtedChangeHistory.Should().BeFalse();
            latestOrganisationDetails.Should().BeNull();
        }

        [TestMethod]
        public async Task When_GrantRegistrationSubmission_Post_Success_Then_Should_NOT_Update_OrganisationDetailsChangeHistory_InSession()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);

            bool upadtedChangeHistory =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestOrganisationDetails);
            upadtedChangeHistory.Should().BeFalse();
        }

        [TestMethod]
        public async Task When_GrantRegistrationSubmission_Post_Fail_Then_Should_Not_Update_OrganisationDetailsChangeHistory_InSession()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession.SelectedRegistrations =
                new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> { { submissionId, detailsModel } };
            _facadeServiceMock.Setup(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Fail);

            // Act
            var result = await _controller.GrantRegistrationSubmission(new GrantRegistrationSubmissionViewModel
            { SubmissionId = submissionId, IsGrantRegistrationConfirmed = true }) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);

            _facadeServiceMock.Verify(r => r.SubmitRegulatorRegistrationDecisionAsync(It.IsAny<RegulatorDecisionRequest>()), Times.AtMostOnce);

            bool upadtedChangeHistory =
                _journeySession.RegulatorRegistrationSubmissionSession.OrganisationDetailsChangeHistory.TryGetValue(submissionId, out var latestOrganisationDetails);

            upadtedChangeHistory.Should().BeFalse();
            latestOrganisationDetails.Should().BeNull();
        }

        #endregion OrganisationDetailsChangeHistorySession

        #region RegistrationSubmissionsFileDownload

        [TestMethod]
        public async Task RegistrationSubmissionsFileDownload_ValidDownloadType_ShouldRedirectToSubmissionDetailsFileDownload()
        {
            // Arrange
            string downloadType = "TestDownloadType";
            string expectedRedirectAction = nameof(RegistrationSubmissionsController.SubmissionDetailsFileDownload);
            string expectedRedirectController = "RegistrationSubmissions";

            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            SetupJourneySession(null, null);

            // Act
            var result = await _controller.RegistrationSubmissionsFileDownload(downloadType, Guid.NewGuid()) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedRedirectAction, result.ActionName, "Action name should match");
            Assert.AreEqual(expectedRedirectController, result.ControllerName, "Controller name should match");

            _mockSessionManager.Verify(manager => manager.GetSessionAsync(It.IsAny<ISession>()), Times.Once, "Session should be retrieved once");
            _mockSessionManager.Verify(manager => manager.SaveSessionAsync(It.IsAny<ISession>(), _journeySession), Times.Once, "Session should be saved once");

            Assert.AreEqual(downloadType, _journeySession.RegulatorRegistrationSubmissionSession.FileDownloadRequestType, "FileDownloadRequestType should match");

            tempDataMock.VerifySet(tempData => tempData["DownloadCompleted"] = false, Times.Once, "DownloadCompleted should be set to false in TempData");
        }

        [TestMethod]
        public async Task RegistrationSubmissionsFileDownload_NullDownloadType_ShouldHandleGracefully()
        {
            // Arrange
            string downloadType = null; // Simulate null input
            string expectedRedirectAction = nameof(RegistrationSubmissionsController.SubmissionDetailsFileDownload);

            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            SetupJourneySession(null, null);

            // Act
            var result = await _controller.RegistrationSubmissionsFileDownload(downloadType, Guid.NewGuid()) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedRedirectAction, result.ActionName, "Action name should match");

            _mockSessionManager.Verify(manager => manager.GetSessionAsync(It.IsAny<ISession>()), Times.Once, "Session should be retrieved once");
            _mockSessionManager.Verify(manager => manager.SaveSessionAsync(It.IsAny<ISession>(), _journeySession), Times.Once, "Session should be saved once");
            Assert.IsNull(_journeySession.RegulatorRegistrationSubmissionSession.FileDownloadRequestType, "FileDownloadRequestType should be null");

            tempDataMock.VerifySet(tempData => tempData["DownloadCompleted"] = false, Times.Once, "DownloadCompleted should be set to false in TempData");
        }

        #endregion RegistrationSubmissionsFileDownload

        #region FileDownloadInProgress

        [TestMethod]
        public async Task FileDownloadInProgress_Should_LogAndRedirectToPageNotFound_SubmissionIsNull()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            string expectedRedirectAction = nameof(RegistrationSubmissionsController.PageNotFound);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { Guid.NewGuid(), detailsModel }
                }
            };

            // Act
            var result = await _controller.FileDownloadInProgress(submissionId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedRedirectAction, result.ActionName, "Action name should match");
            _loggerMock.Verify(
                        x => x.Log(
                            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                            It.IsAny<EventId>(),
                            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"{submissionId} - submission not found - ")),
                            It.IsAny<Exception>(),
                            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                        Times.Once);
            _facadeServiceMock.Verify(m => m.GetFileDownload(It.IsAny<FileDownloadRequest>()), Times.Never);
        }

        [TestMethod]
        public async Task FileDownloadInProgress_ShouldRedirectToFileDownloadFailed_WhenFileDownloadModelIsNull()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            string expectedRedirectAction = nameof(RegistrationSubmissionsController.RegistrationSubmissionFileDownloadFailed);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            // Act
            var result = await _controller.FileDownloadInProgress(submissionId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedRedirectAction, result.ActionName, "Action name should match");

            _facadeServiceMock.Verify(m => m.GetFileDownload(It.IsAny<FileDownloadRequest>()), Times.Never);

        }

        [TestMethod]
        [DataRow(HttpStatusCode.Forbidden, nameof(RegistrationSubmissionsController.RegistrationSubmissionFileDownloadSecurityWarning))]
        [DataRow(HttpStatusCode.BadRequest, nameof(RegistrationSubmissionsController.RegistrationSubmissionFileDownloadFailed))]
        public async Task FileDownloadInProgress_ShouldRedirectCorrectly_WhenFacadeDoesNotReturnSuccess(
            HttpStatusCode httpStatusCode,
            string expectedRedirectAction)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string fileName = "test-file.csv";
            var fileId = Guid.NewGuid();
            string blobName = "test-blob";

            // Generate mock registration details with files
            var registration = new RegistrationSubmissionOrganisationDetails
            {
                SubmissionId = submissionId,
                OrganisationReference = "ORG12345",
                SubmissionDetails = new RegistrationSubmissionOrganisationSubmissionSummaryDetails
                {
                    Files =
            [
                new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
                {
                    FileId = fileId,
                    BlobName = blobName,
                    FileName = fileName,
                    Type = RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company
                }
            ]
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Set the file download request type in session to OrganisationDetails
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>
                    { { submissionId, registration } },
                FileDownloadRequestType = FileDownloadTypes.OrganisationDetails
            };

            // Mock the CreateFileDownloadRequest method
            var mockedFileDownloadRequest = new FileDownloadRequest
            {
                SubmissionId = submissionId,
                FileId = fileId,
                BlobName = blobName,
                FileName = fileName,
                SubmissionType = SubmissionType.Registration
            };

            // Mock the facade service to return an usuccessful response with content
            var fileStream = new MemoryStream();
            var response = new HttpResponseMessage(httpStatusCode)
            {
                Content = new StreamContent(fileStream)
            };

            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            _facadeServiceMock
                .Setup(mock => mock.GetFileDownload(It.Is<FileDownloadRequest>(req =>
                    req.SubmissionId == mockedFileDownloadRequest.SubmissionId &&
                    req.FileId == mockedFileDownloadRequest.FileId &&
                    req.BlobName == mockedFileDownloadRequest.BlobName &&
                    req.FileName == mockedFileDownloadRequest.FileName &&
                    req.SubmissionType == SubmissionType.Registration)))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.FileDownloadInProgress(submissionId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(expectedRedirectAction, result.ActionName, "Action name should match");

            _facadeServiceMock.Verify(m => m.GetFileDownload(It.IsAny<FileDownloadRequest>()), Times.AtMostOnce);

            response.Dispose();
        }

        [TestMethod]
        [DataRow(RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company, FileDownloadTypes.OrganisationDetails)]
        [DataRow(RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.brands, FileDownloadTypes.BrandDetails)]
        [DataRow(RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.partnership, FileDownloadTypes.PartnershipDetails)]
        public async Task FileDownloadInProgress_ShouldReturnFileStreamResult_WhenFacadeReturnsSuccess(
            RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType fileType,
            string fileDownloadTypes)
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            string fileName = "test-file.csv";
            var fileId = Guid.NewGuid();
            string blobName = "test-blob";

            // Generate mock registration details with files
            var registration = new RegistrationSubmissionOrganisationDetails
            {
                SubmissionId = submissionId,
                OrganisationReference = "ORG12345",
                SubmissionDetails = new RegistrationSubmissionOrganisationSubmissionSummaryDetails
                {
                    Files =
            [
                new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
                {
                    FileId = fileId,
                    BlobName = blobName,
                    FileName = fileName,
                    Type = fileType
                }
            ]
                }
            };

            var tempDataMock = new Mock<ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;

            // Set the file download request type in session
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails>
                    { { submissionId, registration } },
                FileDownloadRequestType = fileDownloadTypes
            };

            // Mock the CreateFileDownloadRequest method
            var mockedFileDownloadRequest = new FileDownloadRequest
            {
                SubmissionId = submissionId,
                FileId = fileId,
                BlobName = blobName,
                FileName = fileName,
                SubmissionType = SubmissionType.Registration
            };


            // Mock the facade service to return a successful response with content
            var fileStream = new MemoryStream();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(fileStream)
            };

            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            _facadeServiceMock
                .Setup(mock => mock.GetFileDownload(It.Is<FileDownloadRequest>(req =>
                    req.SubmissionId == mockedFileDownloadRequest.SubmissionId &&
                    req.FileId == mockedFileDownloadRequest.FileId &&
                    req.BlobName == mockedFileDownloadRequest.BlobName &&
                    req.FileName == mockedFileDownloadRequest.FileName &&
                    req.SubmissionType == SubmissionType.Registration)))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.FileDownloadInProgress(submissionId) as FileStreamResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual("application/octet-stream", result.ContentType, "ContentType should be 'application/octet-stream'.");
            Assert.AreEqual(fileName, result.FileDownloadName, "File name should match the expected file name.");

            // Verify that the facade service was called once
            _facadeServiceMock.Verify(m => m.GetFileDownload(It.IsAny<FileDownloadRequest>()), Times.Once);

            // Verify that DownloadCompleted is set to true in TempData
            tempDataMock.VerifySet(tempData => tempData["DownloadCompleted"] = true, Times.Once, "DownloadCompleted should be set to true in TempData");

            // Clean up
            response.Dispose();
        }

        #endregion FileDownloadInProgress

        #region SubmissionDetailsFileDownload

        [TestMethod]
        public async Task SubmissionDetailsFileDownload_ShouldReturnViewResult()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            // Act
            var result = await _controller.SubmissionDetailsFileDownload(submissionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult), "The result should be a ViewResult.");
            var viewResult = result as ViewResult;
            Assert.AreEqual("RegistrationSubmissionFileDownload", viewResult.ViewName, "The view name should be 'RegistrationSubmissionFileDownload'.");
        }

        #endregion SubmissionDetailsFileDownload

        #region RegistrationSubmissionFileDownloadFailed

        [TestMethod]
        public async Task RegistrationSubmissionFileDownloadFailed_ShouldReturnViewWithCorrectModel()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var expectedModel = new OrganisationDetailsFileDownloadViewModel
            {
                DownloadFailed = true,
                HasIssue = false,
                SubmissionId = submissionId
            };

            // Act
            var result = await _controller.RegistrationSubmissionFileDownloadFailed(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual("RegistrationSubmissionFileDownloadFailed", result.ViewName, "The view name should be 'RegistrationSubmissionFileDownloadFailed'.");

            // Assert that the model is of the correct type
            var model = result.Model as OrganisationDetailsFileDownloadViewModel;
            Assert.IsNotNull(model, "The model should not be null.");
            Assert.AreEqual(expectedModel.DownloadFailed, model.DownloadFailed, "DownloadFailed should be true.");
            Assert.AreEqual(expectedModel.HasIssue, model.HasIssue, "HasIssue should be false.");
            Assert.AreEqual(expectedModel.SubmissionId, model.SubmissionId, "The SubmissionId should match.");
        }

        #endregion RegistrationSubmissionFileDownloadFailed

        #region RegistrationSubmissionFileDownloadSecurityWarning

        [TestMethod]
        public async Task RegistrationSubmissionFileDownloadSecurityWarning_ShouldReturnViewWithCorrectModel()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var detailsModel = GenerateTestSubmissionDetailsViewModel(submissionId);
            _journeySession.RegulatorRegistrationSubmissionSession = new RegulatorRegistrationSubmissionSession
            {
                SelectedRegistrations = new Dictionary<Guid, RegistrationSubmissionOrganisationDetails> {
                    { submissionId, detailsModel }
                }
            };

            var expectedModel = new OrganisationDetailsFileDownloadViewModel
            {
                DownloadFailed = true,
                HasIssue = true,
                SubmissionId = submissionId
            };

            // Act
            var result = await _controller.RegistrationSubmissionFileDownloadSecurityWarning(submissionId) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual("RegistrationSubmissionFileDownloadFailed", result.ViewName, "The view name should be 'RegistrationSubmissionFileDownloadFailed'.");

            // Assert that the model is of the correct type
            var model = result.Model as OrganisationDetailsFileDownloadViewModel;
            Assert.IsNotNull(model, "The model should not be null.");
            Assert.AreEqual(expectedModel.DownloadFailed, model.DownloadFailed, "DownloadFailed should be true.");
            Assert.AreEqual(expectedModel.HasIssue, model.HasIssue, "HasIssue should be true.");
            Assert.AreEqual(expectedModel.SubmissionId, model.SubmissionId, "The SubmissionId should match.");
        }

        #endregion RegistrationSubmissionFileDownloadSecurityWarning

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