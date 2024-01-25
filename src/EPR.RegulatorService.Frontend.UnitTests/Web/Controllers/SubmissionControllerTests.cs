using AutoFixture;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class SubmissionControllerTests : SubmissionsTestBase
    {
        private const int DefaultPageNumber = 1;
        private const int PageNumberTwo = 2;
        private const string SearchOrganisationName = "Test Organisation";
        private const string UserName = "Test User";
        private Fixture _fixture;
        
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            
            JourneySessionMock = new JourneySession()
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession()
                {
                    SearchOrganisationName = SearchOrganisationName,
                    SearchOrganisationId = string.Empty,
                    IsDirectProducerChecked = true,
                    IsComplianceSchemeChecked = true,
                    IsPendingSubmissionChecked = true,
                    IsAcceptedSubmissionChecked = true,
                    IsRejectedSubmissionChecked = true,
                    CurrentPageNumber = 1
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            _fixture = new Fixture();
        }

        [TestMethod]
        public async Task Submissions_WithNullSession_CreatesNewSessionAndReturnsView()
        {
            // Act
            var result = await _systemUnderTest.Submissions();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();

            var viewModel = viewResult.Model as SubmissionsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
        }
        
        [TestMethod]
        public async Task GivenOnSubmissionsGet_WithValidSession_WhenSubmissionAccepted_ThenUpdatesSession_AndGetsValuesFromTempData_AndReturnsView()
        {
            //Arrange
            _systemUnderTest.TempData["SubmissionResultAccept"] = EndpointResponseStatus.Success;
            _systemUnderTest.TempData["SubmissionResultOrganisationName"] = SearchOrganisationName;

            // Act
            var result = await _systemUnderTest.Submissions();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();

            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.AcceptSubmissionResult.Should().Be(EndpointResponseStatus.Success);
            viewModel.RejectSubmissionResult.Should().Be(EndpointResponseStatus.NotSet);
            viewModel.OrganisationName.Should().Be(SearchOrganisationName);
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }
        
        [TestMethod]
        public async Task GivenOnSubmissionsGet_WithValidSession_WhenSubmissionRejected_ThenUpdatesSession_AndGetsValuesFromTempData_AndReturnsView()
        {
            //Arrange
            _systemUnderTest.TempData["SubmissionResultReject"] = EndpointResponseStatus.Success;
            _systemUnderTest.TempData["SubmissionResultOrganisationName"] = SearchOrganisationName;

            // Act
            var result = await _systemUnderTest.Submissions();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();

            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.RejectSubmissionResult.Should().Be(EndpointResponseStatus.Success);
            viewModel.AcceptSubmissionResult.Should().Be(EndpointResponseStatus.NotSet);
            viewModel.OrganisationName.Should().Be(SearchOrganisationName);
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }
        
        [TestMethod]
        public async Task GivenOnSubmissionsGet_WithValidSession_WhenPageTwoSelected_ThenUpdatesSessionAndReturnsView()
        {
            // Act
            var result = await _systemUnderTest.Submissions(PageNumberTwo);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();

            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(PageNumberTwo);
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(PageNumberTwo);
            
            _systemUnderTest.TempData["SubmissionResultAccept"].Should().BeNull();
            _systemUnderTest.TempData["SubmissionResultReject"].Should().BeNull();
            _systemUnderTest.TempData["SubmissionResultOrganisationName"].Should().BeNull();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SelectedFiltersOverwriteSessionFilters_And_IsFilteredSearch()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationName = SearchOrganisationName;
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationId = string.Empty;
            JourneySessionMock.RegulatorSubmissionSession.IsDirectProducerChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsComplianceSchemeChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsPendingSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsAcceptedSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsRejectedSubmissionChecked = false;
            
            // Act
            var result = await _systemUnderTest.Submissions();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(SearchOrganisationName);
            viewModel.IsDirectProducerChecked.Should().BeFalse();
            viewModel.IsComplianceSchemeChecked.Should().BeFalse();
            viewModel.IsPendingSubmissionChecked.Should().BeFalse();
            viewModel.IsAcceptedSubmissionChecked.Should().BeFalse();
            viewModel.IsRejectedSubmissionChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_NoFilterActionApplied_AndNullJsonSubmission()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel());

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("error");
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_NoFilterActionApplied_AndJsonSubmissionNotEmpty()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var submission = _fixture.Create<Submission>();
            var submissionString =  JsonSerializer.Serialize(submission);
            
            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel(), null, submissionString);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("SubmissionDetails");

            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission.OrganisationId.Should().Be(submission.OrganisationId);
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SessionFiltersApplied_And_Not_IsFilteredSearch()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel(), FilterActions.SubmitFilters);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("Submissions");
            
            var session = JourneySessionMock.RegulatorSubmissionSession;
            session.CurrentPageNumber.Should().Be(DefaultPageNumber);
            session.SearchOrganisationName.Should().Be(SearchOrganisationName);
            session.SearchOrganisationId.Should().BeEmpty();
            session.IsDirectProducerChecked.Should().BeTrue();
            session.IsComplianceSchemeChecked.Should().BeTrue();
            session.IsPendingSubmissionChecked.Should().BeTrue();
            session.IsAcceptedSubmissionChecked.Should().BeTrue();
            session.IsRejectedSubmissionChecked.Should().BeTrue();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_FiltersCleared()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel{ClearFilters = true}, FilterActions.ClearFilters);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be(nameof(SubmissionsController.Submissions));

            var session = JourneySessionMock.RegulatorSubmissionSession;
            session.CurrentPageNumber.Should().Be(DefaultPageNumber);
            session.SearchOrganisationName.Should().BeEmpty();
            session.SearchOrganisationId.Should().BeEmpty();
            session.IsDirectProducerChecked.Should().BeFalse();
            session.IsComplianceSchemeChecked.Should().BeFalse();
            session.IsPendingSubmissionChecked.Should().BeFalse();
            session.IsAcceptedSubmissionChecked.Should().BeFalse();
            session.IsRejectedSubmissionChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_RedirectedFromSecondPage()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationName = SearchOrganisationName;
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationId = string.Empty;
            JourneySessionMock.RegulatorSubmissionSession.IsDirectProducerChecked = true;
            JourneySessionMock.RegulatorSubmissionSession.IsComplianceSchemeChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsPendingSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsAcceptedSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsRejectedSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber = 3;

            // Act
            var result = await _systemUnderTest.Submissions(2);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(PageNumberTwo);
            viewModel.SearchOrganisationName.Should().Be(SearchOrganisationName);
            viewModel.SearchOrganisationId.Should().BeEmpty();
            viewModel.IsDirectProducerChecked.Should().BeTrue();
            viewModel.IsComplianceSchemeChecked.Should().BeFalse();
            viewModel.IsPendingSubmissionChecked.Should().BeFalse();
            viewModel.IsAcceptedSubmissionChecked.Should().BeFalse();
            viewModel.IsRejectedSubmissionChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public void Given_TimeAndDateForSubmission_Set_Then_FormatTimeAndDateForSubmission_As_String()
        {
            DateTime timeAndDateOfSubmission = new(2023, 11, 11, 13, 11, 11);
            string result = _systemUnderTest.FormatTimeAndDateForSubmission(timeAndDateOfSubmission);

            result.Should().BeEquivalentTo("1:11pm on 11 November 2023");
        }

        [TestMethod]
        public async Task Submissions_WithNullSession_CreatesNewSessionAndRedirectsToSubmissionDetails()
        {
            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel(), null, "{\r\n  \"SubmissionId\": \"a3f6c7b8-9d4e-4f9a-bcde-1234567890ab\",\r\n  \"SubmissionDate\": \"2023-10-30T10:51:23Z\",\r\n  \"SubmissionStatus\": \"Pending\",\r\n  \"IsResubmission\": false,\r\n  \"IsResubmissionRequired\": false,\r\n  \"RejectionComments\": \"\",\r\n  \"Organisation\": {\r\n    \"OrganisationId\": \"d4e5f6a7-b8c9-0d1e-2f3a-9876543210cd\",\r\n    \"OrganisationName\": \"Acme Inc.\",\r\n    \"OrganisationType\": \"Private\"\r\n  }\r\n}\r\n");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            actionResult?.ActionName.Should().Be("SubmissionDetails");
        }
        
        [TestMethod]
        public async Task GivenOnSubmissionDetailsPost_WhenJourneyTypeIsAccept_ThenRedirectToAcceptSubmission()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act
            var result = await _systemUnderTest.SubmissionDetails(new SubmissionDetailsViewModel(), JourneyType.Accept);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("AcceptSubmission");
        }
        
        [TestMethod]
        public async Task GivenOnSubmissionDetailsPost_WhenJourneyTypeIsReject_ThenSaveSession_AndRedirectToRejectSubmission()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var submissionDetailsViewModel = new SubmissionDetailsViewModel
            {
                OrganisationName = SearchOrganisationName, SubmissionId = Guid.NewGuid(), SubmittedBy = UserName
            };
            
            // Act
            var result = await _systemUnderTest.SubmissionDetails(submissionDetailsViewModel, JourneyType.Reject);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("RejectSubmission");

            JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData.OrganisationName.Should().Be(submissionDetailsViewModel.OrganisationName);
            JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmissionId.Should().Be(submissionDetailsViewModel.SubmissionId);
            JourneySessionMock.RegulatorSubmissionSession.RejectSubmissionJourneyData.SubmittedBy.Should().Be(submissionDetailsViewModel.SubmittedBy);
        }

        [TestMethod]
        public async Task GivenSetOrResetFilterValuesInSession_WhenClearFilters_ThenUpdatesFiltersInSession()
        {
            // Arrange
            var submissionFiltersModel = new SubmissionFiltersModel { ClearFilters = true };
            
            // Act
            _systemUnderTest.SetOrResetFilterValuesInSession(JourneySessionMock, submissionFiltersModel);
            
            //Assert
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationName.Should().Be(string.Empty);
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationId.Should().Be(string.Empty);
            JourneySessionMock.RegulatorSubmissionSession.IsDirectProducerChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsComplianceSchemeChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsPendingSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsAcceptedSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsRejectedSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }
        
        [TestMethod]
        public async Task GivenSetOrResetFilterValuesInSession_WhenNotClearFilters_AndIsFilterable_ThenUpdatesFiltersInSession()
        {
            // Arrange
            var submissionFiltersModel = new SubmissionFiltersModel
            {
                ClearFilters = false,
                IsFilteredSearch = true,
                SearchOrganisationName = SearchOrganisationName
            };
            
            // Act
            _systemUnderTest.SetOrResetFilterValuesInSession(JourneySessionMock, submissionFiltersModel);
            
            // Assert
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationName.Should().Be(SearchOrganisationName);
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationId.Should().Be(string.Empty);
            JourneySessionMock.RegulatorSubmissionSession.IsDirectProducerChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsComplianceSchemeChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsPendingSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsAcceptedSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.IsRejectedSubmissionChecked.Should().BeFalse();
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }
    }
}