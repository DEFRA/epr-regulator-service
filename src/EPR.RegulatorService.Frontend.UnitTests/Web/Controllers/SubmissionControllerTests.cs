using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class SubmissionControllerTests : SubmissionsTestBase
    {
        private const int DefaultPageNumber = 1;
        private const int PageNumberTwo = 2;
        private const string SearchOrganisationName = "Test Organisation";
        
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            
            JourneySessionMock = new JourneySession()
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession()
                {
                    SubmissionFiltersModel = new SubmissionFiltersModel()
                    {
                        IsFilteredSearch = false,
                        ClearFilters = false,
                        SearchOrganisationName = SearchOrganisationName,
                        SearchOrganisationId = string.Empty,
                        IsDirectProducerChecked = true,
                        IsComplianceSchemeChecked = true,
                        IsPendingSubmissionChecked = true,
                        IsAcceptedSubmissionChecked = true,
                        IsRejectedSubmissionChecked = true
                    },
                    PageNumber = 1
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task Submissions_WithNullSession_CreatesNewSessionAndReturnsView()
        {
            // Arrange
            var submissionFiltersModel = new SubmissionFiltersModel
            {
                IsFilteredSearch = true,
                ClearFilters = false
            };
            
            // Act
            var result = await _systemUnderTest.Submissions(submissionFiltersModel);

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
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SelectedFiltersOverwriteSessionFilters_And_IsFilteredSearch()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var submissionsFilterModel = new SubmissionFiltersModel()
            {
                SearchOrganisationName = SearchOrganisationName,
            };
            
            var result = await _systemUnderTest.Submissions(submissionsFilterModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SubmissionFilters.SearchOrganisationName.Should().Be(SearchOrganisationName);
            viewModel.SubmissionFilters.IsDirectProducerChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsComplianceSchemeChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsPendingSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsAcceptedSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsRejectedSubmissionChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SessionFiltersApplied_And_Not_IsFilteredSearch()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            var result = await _systemUnderTest.Submissions(new SubmissionFiltersModel());

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SubmissionFilters.SearchOrganisationName.Should().Be(SearchOrganisationName);
            viewModel.SubmissionFilters.SearchOrganisationId.Should().BeEmpty();
            viewModel.SubmissionFilters.IsDirectProducerChecked.Should().BeTrue();
            viewModel.SubmissionFilters.IsComplianceSchemeChecked.Should().BeTrue();
            viewModel.SubmissionFilters.IsPendingSubmissionChecked.Should().BeTrue();
            viewModel.SubmissionFilters.IsAcceptedSubmissionChecked.Should().BeTrue();
            viewModel.SubmissionFilters.IsRejectedSubmissionChecked.Should().BeTrue();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_FiltersCleared()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            var result = await _systemUnderTest.Submissions(new SubmissionFiltersModel{ClearFilters = true});

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SubmissionFilters.SearchOrganisationName.Should().BeEmpty();
            viewModel.SubmissionFilters.SearchOrganisationId.Should().BeEmpty();
            viewModel.SubmissionFilters.IsDirectProducerChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsComplianceSchemeChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsPendingSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsAcceptedSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsRejectedSubmissionChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_RedirectedFromSecondPage()
        {
            // Arrange
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
            
            var submissionsFilterModel = new SubmissionFiltersModel()
            {
                SearchOrganisationName = SearchOrganisationName,
                IsDirectProducerChecked = true
            };
            
            var result = await _systemUnderTest.Submissions(submissionsFilterModel, pageNumber:2);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<SubmissionsViewModel>();
            var viewModel = viewResult.Model as SubmissionsViewModel;
            viewModel.PageNumber.Should().Be(PageNumberTwo);
            viewModel.SubmissionFilters.SearchOrganisationName.Should().Be(SearchOrganisationName);
            viewModel.SubmissionFilters.SearchOrganisationId.Should().BeEmpty();
            viewModel.SubmissionFilters.IsDirectProducerChecked.Should().BeTrue();
            viewModel.SubmissionFilters.IsComplianceSchemeChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsPendingSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsAcceptedSubmissionChecked.Should().BeFalse();
            viewModel.SubmissionFilters.IsRejectedSubmissionChecked.Should().BeFalse();
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
            var result = await _systemUnderTest.Submissions("{\r\n  \"SubmissionId\": \"a3f6c7b8-9d4e-4f9a-bcde-1234567890ab\",\r\n  \"SubmissionDate\": \"2023-10-30T10:51:23Z\",\r\n  \"SubmissionStatus\": \"Pending\",\r\n  \"IsResubmission\": false,\r\n  \"IsResubmissionRequired\": false,\r\n  \"RejectionComments\": \"\",\r\n  \"Organisation\": {\r\n    \"OrganisationId\": \"d4e5f6a7-b8c9-0d1e-2f3a-9876543210cd\",\r\n    \"OrganisationName\": \"Acme Inc.\",\r\n    \"OrganisationType\": \"Private\"\r\n  }\r\n}\r\n");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            actionResult?.ControllerName.Should().Be("Submissions");
            actionResult?.ActionName.Should().Be("SubmissionDetails");

        }
    }
}