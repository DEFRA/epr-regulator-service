using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Submissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.Submissions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class SubmissionControllerTests : SubmissionsTestBase
    {
        private const int DefaultPageNumber = 1;
        private const int PageNumberTwo = 2;
        private const string SearchOrganisationName = "Test Organisation";
        private const string DefaultOfflinePaymentAmount = "10.00";
        private const string DefaultNationCode = "GB-ENG";
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

            _submissionServiceMock.Setup(x => x.GetFilteredSubmissionYearsAndPeriods())
                .Returns(([2023, 2024],
                    ["January to June 2023",
                    "July to December 2023",
                    "January to June 2024",
                    "July to December 2024"]));

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
            viewModel.SubmissionYears.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.Years);
            viewModel.SubmissionPeriods.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.PomPeriods);
            viewModel.SearchSubmissionYears.Should().BeNull();
            viewModel.SearchSubmissionPeriods.Should().BeNull();
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

            _submissionServiceMock.Setup(x => x.GetFilteredSubmissionYearsAndPeriods())
                .Returns(([2023, 2024],
                    ["January to June 2023",
                    "July to December 2023",
                    "January to June 2024",
                    "July to December 2024"]));

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
            viewModel.SubmissionYears.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.Years);
            viewModel.SubmissionPeriods.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.PomPeriods);
            viewModel.SearchSubmissionYears.Should().BeNull();
            viewModel.SearchSubmissionPeriods.Should().BeNull();
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }

        [TestMethod]
        public async Task GivenOnSubmissionsGet_WithValidSession_WhenPageTwoSelected_ThenUpdatesSessionAndReturnsView()
        {
            // Arrange
            _submissionServiceMock.Setup(x => x.GetFilteredSubmissionYearsAndPeriods())
                .Returns(([2023, 2024],
                    ["January to June 2023",
                    "July to December 2023",
                    "January to June 2024",
                    "July to December 2024"]));

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
            viewModel.SubmissionYears.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.Years);
            viewModel.SubmissionPeriods.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.PomPeriods);
            viewModel.SearchSubmissionYears.Should().BeNull();
            viewModel.SearchSubmissionPeriods.Should().BeNull();
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

            _submissionServiceMock.Setup(x => x.GetFilteredSubmissionYearsAndPeriods())
                .Returns(([2023, 2024],
                    ["January to June 2023",
                    "July to December 2023",
                    "January to June 2024",
                    "July to December 2024"]));

            int[] searchedSubmissionYears = [2023];
            string[] searchedSubmissionPeriods = ["Jan to June 2023"];

            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationName = SearchOrganisationName;
            JourneySessionMock.RegulatorSubmissionSession.SearchOrganisationId = string.Empty;
            JourneySessionMock.RegulatorSubmissionSession.IsDirectProducerChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsComplianceSchemeChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsPendingSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsAcceptedSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.IsRejectedSubmissionChecked = false;
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionYears = searchedSubmissionYears;
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionPeriods = searchedSubmissionPeriods;

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
            viewModel.SubmissionYears.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.Years);
            viewModel.SubmissionPeriods.Should().BeEquivalentTo(_submissionFiltersMock.Object.Value.PomPeriods);
            viewModel.SearchSubmissionYears.Should().BeEquivalentTo(searchedSubmissionYears);
            viewModel.SearchSubmissionPeriods.Should().BeEquivalentTo(searchedSubmissionPeriods);
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
            string submissionString = JsonSerializer.Serialize(submission);

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
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel { ClearFilters = true }, FilterActions.ClearFilters);

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
        public async Task Submissions_WithExportTrue_ReturnsCsvFile()
        {
            // Arrange
            var data = new byte[1] { 1 };

            _facadeServiceMock.Setup(x => x.GetPackagingSubmissionsCsv(It.IsAny<GetPackagingSubmissionsCsvRequest>()))
                .ReturnsAsync(new MemoryStream(data));

            // Act
            var result = await _systemUnderTest.Submissions(new SubmissionsRequestViewModel(), null, null, true);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FileStreamResult>();

            var fileStreamResult = result as FileStreamResult;
            fileStreamResult.ContentType.Should().Be("text/csv");
            fileStreamResult.FileDownloadName.Should().Be("packaging-submissions.csv");
            fileStreamResult.FileStream.ReadByte().Should().Be(data[0]);
            fileStreamResult.FileStream.ReadByte().Should().Be(-1);
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
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionYears.Should().BeEmpty();
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionPeriods.Should().BeEmpty();
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
                SearchOrganisationName = SearchOrganisationName,
                SearchSubmissionYears = new[] { 2023 },
                SearchSubmissionPeriods = new[] { "Jan to June 2023" }
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
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionYears.Should().BeEquivalentTo(submissionFiltersModel.SearchSubmissionYears);
            JourneySessionMock.RegulatorSubmissionSession.SearchSubmissionPeriods.Should().BeEquivalentTo(submissionFiltersModel.SearchSubmissionPeriods);
            JourneySessionMock.RegulatorSubmissionSession.CurrentPageNumber.Should().Be(DefaultPageNumber);
        }

        [TestMethod]
        public async Task SubmissionsFileDownload_RedirectsToPackagingDataFileDownload()
        {
            // Act
            var result = await _systemUnderTest.SubmissionsFileDownload();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("PackagingDataFileDownload");
            redirectResult.ControllerName.Should().Be("Submissions");
        }

        [TestMethod]
        public void PackagingDataFileDownload_ReturnsViewWithViewModel()
        {
            // Act
            var result = _systemUnderTest.PackagingDataFileDownload();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            ((Microsoft.AspNetCore.Mvc.ViewResult)result).ViewName.Should().Be("PackagingDataFileDownload");
        }

        [TestMethod]
        public void PackagingDataFileDownloadFailed_ReturnsViewWithViewModel()
        {
            // Act
            var result = _systemUnderTest.PackagingDataFileDownloadFailed();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<SubmissionDetailsFileDownloadViewModel>();
            var model = (SubmissionDetailsFileDownloadViewModel)viewResult.Model;
            model.DownloadFailed.Should().BeTrue();
            model.HasIssue.Should().BeFalse();
            viewResult.ViewName.Should().Be("PackagingDataFileDownloadFailed");
        }


        [TestMethod]
        public async Task PackagingDataFileDownloadSecurityWarning_ReturnsViewWithViewModel()
        {
            // Arrange
            var submission = _fixture.Create<Submission>();
            submission.FirstName = "fstName";
            submission.LastName = "lstName";
            var session = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    OrganisationSubmission = submission,


                }
            };

            _sessionManagerMock
                .Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.PackagingDataFileDownloadSecurityWarning();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<SubmissionDetailsFileDownloadViewModel>();

            var model = (SubmissionDetailsFileDownloadViewModel)viewResult.Model;
            model.DownloadFailed.Should().BeTrue();
            model.HasIssue.Should().BeTrue();
            model.SubmittedBy.Should().Be("fstName lstName"); // Validate the submittedBy field

            viewResult.ViewName.Should().Be("PackagingDataFileDownloadFailed");
        }


        [TestMethod]
        public async Task FileDownloadInProgress_ValidRequest_ReturnsFile()
        {
            // Arrange
            var submission = _fixture.Create<Submission>();
            var session = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    OrganisationSubmission = submission
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            using (var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("file content")
            })
            {
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "testfile.txt"
                };

                _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

                // Act
                var result = await _systemUnderTest.FileDownloadInProgress();

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<FileStreamResult>();
                var fileResult = result as FileStreamResult;
                fileResult.ContentType.Should().Be("application/octet-stream");
                fileResult.FileDownloadName.Should().Be("testfile.txt");
            }
        }

        [TestMethod]
        public async Task FileDownloadInProgress_FileInfected_ReturnsRedirectToAction()
        {
            // Arrange
            var submission = _fixture.Create<Submission>();
            var session = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    OrganisationSubmission = submission
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            using var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);

            _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

            // Act
            var result = await _systemUnderTest.FileDownloadInProgress();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("PackagingDataFileDownloadSecurityWarning");
            redirectResult.ControllerName.Should().BeNull();
        }


        [TestMethod]
        public async Task FileDownloadInProgress_DownloadFailed_ReturnsRedirectToAction()
        {
            // Arrange
            var submission = _fixture.Create<Submission>();
            var session = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    OrganisationSubmission = submission
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            using (var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest))
            {
                _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

                // Act
                var result = await _systemUnderTest.FileDownloadInProgress();

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("PackagingDataFileDownloadFailed");
                redirectResult.ControllerName.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task FileDownloadInProgress_DownloadFailed_ReturnsRedirectToActionXXXXXX()
        {
            // Arrange
            var submission = _fixture.Create<Submission>();
            submission.PomBlobName = null;
            var session = new JourneySession
            {
                RegulatorSubmissionSession = new RegulatorSubmissionSession
                {
                    OrganisationSubmission = submission
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            using (var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest))
            {
                _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

                // Act
                var result = await _systemUnderTest.FileDownloadInProgress();

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("PackagingDataFileDownloadFailed");
                redirectResult.ControllerName.Should().BeNull();
            }
        }

        [TestMethod]
        public void FormatTimeAndDateForSubmission_ReturnsCorrectFormat_MorningTime()
        {
            // Arrange

            var inputDate = new DateTime(2025, 1, 8, 9, 30, 0); // 9:30 AM, 8th January 2025
            var expectedOutput = "9:30am on 08 January 2025";

            // Act
            var result = _systemUnderTest.FormatTimeAndDateForSubmission(inputDate);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [TestMethod]
        public void FormatTimeAndDateForSubmission_ReturnsCorrectFormat_AfternoonTime()
        {
            // Arrange
            var inputDate = new DateTime(2025, 1, 8, 14, 15, 0); // 2:15 PM, 8th January 2025
            var expectedOutput = "2:15pm on 08 January 2025";

            // Act
            var result = _systemUnderTest.FormatTimeAndDateForSubmission(inputDate);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [TestMethod]
        public void FormatTimeAndDateForSubmission_ReturnsCorrectFormat_EdgeCaseMidnight()
        {
            // Arrange
            var inputDate = new DateTime(2025, 1, 8, 0, 0, 0); // Midnight, 8th January 2025
            var expectedOutput = "12:00am on 08 January 2025";

            // Act
            var result = _systemUnderTest.FormatTimeAndDateForSubmission(inputDate);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [TestMethod]
        public void FormatTimeAndDateForSubmission_ReturnsCorrectFormat_EdgeCaseNoon()
        {
            // Arrange
            var inputDate = new DateTime(2025, 1, 8, 12, 0, 0); // Noon, 8th January 2025
            var expectedOutput = "12:00pm on 08 January 2025";

            // Act
            var result = _systemUnderTest.FormatTimeAndDateForSubmission(inputDate);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [TestMethod]
        public void FormatTimeAndDateForSubmission_ReturnsCorrectFormat_LeapYear()
        {
            // Arrange
            var inputDate = new DateTime(2024, 2, 29, 8, 45, 0); // 8:45 AM, 29th February 2024 (leap year)
            var expectedOutput = "8:45am on 29 February 2024";

            // Act
            var result = _systemUnderTest.FormatTimeAndDateForSubmission(inputDate);

            // Assert
            result.Should().Be(expectedOutput);
        }

        [TestMethod]
        public async Task Submissions_GetSessionIsNull_CreatesNewSessionAndReturnsView()
        {
            // Arrange
            _sessionManagerMock.Setup(m => m.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

            // Act
            var result = await _systemUnderTest.Submissions();

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SubmissionsViewModel));
        }

        #region ConfirmOfflinePaymentSubmission

        #region GET

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_GET_ReturnsViewResult_WithExpectedModel()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId
            };

            _tempDataDictionary["OfflinePaymentAmount"] = DefaultOfflinePaymentAmount;

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be(nameof(_systemUnderTest.ConfirmOfflinePaymentSubmission));

            var model = viewResult.Model as ConfirmOfflinePaymentSubmissionViewModel;
            model.Should().NotBeNull();
            model!.SubmissionId.Should().Be(submissionId);
            model.OfflinePaymentAmount.Should().Be(DefaultOfflinePaymentAmount);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_GET_SetsBackLinkCorrectly()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId
            };

            _tempDataDictionary["OfflinePaymentAmount"] = DefaultOfflinePaymentAmount;
            _tempDataDictionary["NationCode"] = DefaultNationCode;

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            AssertBackLink(viewResult!, $"/regulators/{PagePath.SubmissionDetails}");
        }

        #endregion GET

        #region POST

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_POST_ReturnsViewResult_WithInvalidModelState()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId,
                UserId = Guid.NewGuid()
            };

            JourneySessionMock.UserData.Organisations =
            [
               new() {
                   NationId = 1
               }
            ];

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                IsOfflinePaymentConfirmed = null, // Invalid state
                OfflinePaymentAmount = null
            };
            _systemUnderTest.ModelState.AddModelError("Key", "Invalid state");

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission(model);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be(nameof(_systemUnderTest.ConfirmOfflinePaymentSubmission));
            viewResult.Model.Should().Be(model);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_POST_RedirectsToSubmissionDetails_WhenPaymentNotConfirmed()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId,
                UserId = Guid.NewGuid()
            };

            JourneySessionMock.UserData.Organisations =
            [
               new() {
                   NationId = 1
               }
            ];

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                IsOfflinePaymentConfirmed = false
            };

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("SubmissionDetails");
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_POST_RedirectsToError_WhenOfflinePaymentAmountIsNullOrEmpty()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId,
                UserId = Guid.NewGuid()
            };

            JourneySessionMock.UserData.Organisations =
            [
               new() {
                   NationId = 1
               }
            ];

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = null
            };

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be(PagePath.Error);
            redirectResult.ControllerName.Should().Be("Error");
            redirectResult.RouteValues.Should().Contain(new KeyValuePair<string, object>("statusCode", 404));
            redirectResult.RouteValues.Should().Contain(new KeyValuePair<string, object>("backLink", PagePath.SubmissionDetails));
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_POST_RedirectsToSubmissionDetails_WhenOfflinePaymentAmountIsProvided()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId,
                UserId = Guid.NewGuid(),
                ReferenceNumber = "degreg",
                NationCode = "GB-ENG"
            };

            JourneySessionMock.UserData.Organisations =
            [
               new() {
                   NationId = 1
               }
            ];

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = DefaultOfflinePaymentAmount,
            };

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("SubmissionDetails");

            _paymentFacadeServiceMock.Verify(r =>
                r.SubmitOfflinePaymentAsync(
                    It.IsAny<OfflinePaymentRequest>()),
                    Times.AtMostOnce);

            _facadeServiceMock.Verify(r =>
                r.SubmitPackagingDataResubmissionFeePaymentEventAsync(
                    It.IsAny<FeePaymentRequest>()),
                    Times.AtMostOnce);
        }

        [TestMethod]
        public async Task ConfirmOfflinePaymentSubmission_POST_RedirectsToServiceNotAvailable_WhenPaymentFacade_ReturnsNonSuccess()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = new Submission
            {
                SubmissionId = submissionId,
                UserId = Guid.NewGuid()
            };

            JourneySessionMock.UserData.Organisations =
            [
               new() {
                   NationId = 1
               }
            ];

            _paymentFacadeServiceMock.Setup(r =>
                r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>())).ReturnsAsync(EndpointResponseStatus.Fail);

            var model = new ConfirmOfflinePaymentSubmissionViewModel
            {
                IsOfflinePaymentConfirmed = true,
                OfflinePaymentAmount = DefaultOfflinePaymentAmount,
            };

            // Act
            var result = await _systemUnderTest.ConfirmOfflinePaymentSubmission(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);

            // Veryify the redirect URL
            result.RouteName.Should().Be("ServiceNotAvailable");
            _paymentFacadeServiceMock.Verify(r => r.SubmitOfflinePaymentAsync(It.IsAny<OfflinePaymentRequest>()), Times.AtMostOnce);
        }

        #endregion POST

        #endregion ConfirmOfflinePaymentSubmission

        #region SubmitOfflinePayment

        [TestMethod]
        [DataRow(null)]
        [DataRow("0")]
        public async Task SubmitOfflinePayment_ReturnsViewResult_WithInvalidModelState(string offilinePayment)
        {
            // Arrange
            JourneySessionMock.RegulatorSubmissionSession.OrganisationSubmission = TestSubmission.GetTestSubmission();

            var paymentDetailsViewModel = new PaymentDetailsViewModel
            {
                OfflinePayment = offilinePayment
            };
            _systemUnderTest.ModelState.AddModelError("Key", "Invalid state");

            // Act
            var result = await _systemUnderTest.SubmitOfflinePayment(paymentDetailsViewModel);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be(nameof(_systemUnderTest.SubmissionDetails));
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_SetsRequiredFieldsInTempData_WhenValidPaymentAmountProvided()
        {
            // Arrange
            var model = new PaymentDetailsViewModel
            {
                OfflinePayment = DefaultOfflinePaymentAmount, // Valid payment amount
            };

            // Act
            await _systemUnderTest.SubmitOfflinePayment(model);

            // Assert
            _systemUnderTest.TempData["OfflinePaymentAmount"].Should().Be(DefaultOfflinePaymentAmount);
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_FormatsOfflinePaymentAmountCorrectly_WhenValueIsParsable()
        {
            // Arrange
            var model = new PaymentDetailsViewModel
            {
                OfflinePayment = DefaultOfflinePaymentAmount // Input with currency symbol
            };

            // Act
            await _systemUnderTest.SubmitOfflinePayment(model);

            // Assert
            _systemUnderTest.TempData["OfflinePaymentAmount"].Should().Be(DefaultOfflinePaymentAmount);
        }

        [TestMethod]
        public async Task SubmitOfflinePayment_RedirectsToConfirmOfflinePaymentSubmission_WhenModelIsValid()
        {
            // Arrange
            var model = new PaymentDetailsViewModel
            {
                OfflinePayment = "0.0"
            };

            // Act
            var result = await _systemUnderTest.SubmitOfflinePayment(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("ConfirmOfflinePaymentSubmission");
        }

        #endregion SubmitOfflinePayment
    }
}