using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using AutoFixture;

    using RegulatorDecision = Frontend.Web.Constants.RegulatorDecision;
    using ServiceRole = Frontend.Web.Constants.ServiceRole;
    using User = Frontend.Core.Models.User;

    [TestClass]
    public class ApplicationsControllerTests : ApplicationsTestBase
    {
        private const string TransferredOrganisationName = "Organisation to transfer";
        private const string AgencyToTransferTo = "Some nation";
        private const string AcceptedUserFirstName = "King";
        private const string AcceptedUserLastName = "Walter";
        private const string AcceptedUserEmail = "Walter@email.com";
        private const string RejectedUserFirstName = "Barry";
        private const string RejectedUserLastName = "Test";
        private const string RejectedEmail = "btest@test.com";
        private const string DelegatedUserFirstName = "Tom";
        private const string DelegatedUserEmail = "Tom@email.com";
        private const string ModelStateErrorMsg = "Test Model Status Invalid";
        private const string CommentsFieldRequiredErrorMessage = "The Comments field is required.";
        private const string ApprovedUserServiceRole = "Packaging.ApprovedPerson";
        private const string RejectedDecision = "Rejected";
        private readonly Guid _organisationId = Guid.NewGuid();
        private readonly Guid _acceptedUserOrganisationId = Guid.Parse("89fda59e-18e0-47a5-bd20-8ca060a2fa00");
        private const int DefaultPageNumber = 1;
        private const int PageNumberTwo = 2;
        private string _searchString = "test";
        
        private Fixture _fixture;

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            _searchString = "test";

            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
            };
            
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            _fixture = new Fixture();
        }

        [TestMethod]
        public async Task Applications_Sets_FilterValues_In_Session()
        {
            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = true,
                }, FilterActions.SubmitFilters);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();

            JourneySessionMock.RegulatorSession.SearchOrganisationName.Should().Be(_searchString);
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked.Should().BeTrue();
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task Applications_WithNullSession_CreatesNewSessionAndReturnsView()
        {
            // Arrange
            JourneySessionMock = null;
            
            // Act
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
            
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsView()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = _searchString;
            
            // Act
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
        
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFiltersApplied_And_IsFilteredSearch()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = _searchString;
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked = true;
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked = true;
            
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            await _systemUnderTest.Applications(new ApplicationsRequestViewModel()
            {
                IsFilteredSearch = true,
                SearchOrganisationName = _searchString,
                IsApprovedUserTypeChecked = true,
                IsDelegatedUserTypeChecked = false
            }, FilterActions.SubmitFilters);
            
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
        
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFiltersApplied_And_Not_IsFilteredSearch()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = _searchString;
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked = true;
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked = true;
            
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            await _systemUnderTest.Applications(new ApplicationsRequestViewModel()
            {
                IsFilteredSearch = false,
                SearchOrganisationName = null,
                IsApprovedUserTypeChecked = false,
                IsDelegatedUserTypeChecked = false
            }, FilterActions.SubmitFilters);
            
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
        
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeTrue();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFilters_Are_Cleared()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = _searchString;
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked = true;
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked = true;
            
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            await _systemUnderTest.Applications(new ApplicationsRequestViewModel()
            {
                IsFilteredSearch = false,
                SearchOrganisationName = null,
                IsApprovedUserTypeChecked = false,
                IsDelegatedUserTypeChecked = false
            }, FilterActions.ClearFilters);
            
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
        
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(string.Empty);
            viewModel.IsApprovedUserTypeChecked.Should().BeFalse();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_RedirectedFromAnotherPage()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = string.Empty;
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked = true;
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked = false;
            JourneySessionMock.RegulatorSession.CurrentPageNumber = 2;

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            await _systemUnderTest.Applications(new ApplicationsRequestViewModel()
            {
                SearchOrganisationName = null,
                IsApprovedUserTypeChecked = false,
                IsDelegatedUserTypeChecked = false
            }, FilterActions.SubmitFilters);
            
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
        
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.Should().NotBeNull();
            viewModel.PageNumber.Should().Be(PageNumberTwo);
            viewModel.SearchOrganisationName.Should().Be(string.Empty);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }
        
        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_Organisation_Is_Transferred()
        {
            // Arrange
            JourneySessionMock.RegulatorSession.SearchOrganisationName = _searchString;
            JourneySessionMock.RegulatorSession.IsApprovedUserTypeChecked = true;
            JourneySessionMock.RegulatorSession.IsDelegatedUserTypeChecked = true;
            
            _systemUnderTest.TempData["TransferNationResult"]= EndpointResponseStatus.Success;
            _systemUnderTest.TempData["TransferredOrganisationName"]= TransferredOrganisationName;
            _systemUnderTest.TempData["TransferredOrganisationAgency"] = AgencyToTransferTo;
            
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            var result = await _systemUnderTest.Applications();
        
            // Assert
            result.Should().BeOfType<ViewResult>();
            
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
            
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.TransferNationResult.Should().Be(EndpointResponseStatus.Success);
            viewModel.TransferredOrganisationName.Should().Be(TransferredOrganisationName);
            viewModel.TransferredOrganisationAgency.Should().Be(AgencyToTransferTo);
        }
        
        [TestMethod]
        public async Task Applications_When_Organisation_Selected_Redirects_To_EnrolmentRequests_And_OrganisationId_Set_In_Session()
        {
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act 
            var result = await _systemUnderTest.Applications(new ApplicationsRequestViewModel(), null, _organisationId) as RedirectToActionResult;
            
            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().NotBeNull();
            result.ActionName.Should().Be(nameof(ApplicationsController.EnrolmentRequests));
            JourneySessionMock.RegulatorSession.OrganisationId.Should().Be(_organisationId);
        }
        
        [TestMethod]
        public async Task Application_Accepted_SendsEmails_Saved_To_Database_Redirects_With_Success_Banner()
        {
            // Arrange
            var organisationId = _acceptedUserOrganisationId;
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = organisationId
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = ServiceRole.ApprovedPerson } };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = organisationId
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(organisationId))
                .ReturnsAsync(organisationDetails);
        
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            // Act
            var result = await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail, ServiceRole.ApprovedPerson);
        
            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = result as RedirectToActionResult;
            redirectToActionResult.ActionName.Should().Be("EnrolmentRequests");
        }
        
        [TestMethod]
        public async Task? Application_Accepted_But_Email_Failed_Throws()
        {
            // Arrange
            var organisationId = _acceptedUserOrganisationId;
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = organisationId
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = ServiceRole.ApprovedPerson } };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = organisationId,
                BusinessAddress = new BusinessAddress()
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(organisationId))
                .ReturnsAsync(organisationDetails);

            var exception = new Exception("Failed to send email");
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .Throws(exception);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail,
                    ServiceRole.ApprovedPerson));
        }
        
        [TestMethod]
        public async Task? Application_Accepted_But_Database_Save_Failed_Throws()
        {
            // Arrange
            var organisationId = _acceptedUserOrganisationId;
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = organisationId
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = ServiceRole.ApprovedPerson } };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = organisationId,
                BusinessAddress = new BusinessAddress()
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(organisationId))
                .ReturnsAsync(organisationDetails);

            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);

            var exception = new Exception("Failed to save to DB");
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .Throws(exception);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail,
                    ServiceRole.ApprovedPerson));
        }
        
        [TestMethod]
        public async Task? GivenEnrolmentRequestsPost_WhenAcceptJourney_ThenRedirectToAcceptApplication_AndFollowHappyPath()
        {
            // Arrange
            var userEnrolment = _fixture.Build<UserEnrolment>()
                .With(x => x.IsApprovedUserAccepted)
                .With(x => x.User)
                .Create();

            var organisationId = _acceptedUserOrganisationId;
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = organisationId
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            var approvedUser = new User() { Enrolment = new Enrolment { ServiceRole = ServiceRole.ApprovedPerson } };
            var organisationDetails = new OrganisationEnrolments
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = organisationId
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(organisationId))
                .ReturnsAsync(organisationDetails);
        
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
            
            // Act
            var result = await _systemUnderTest.EnrolmentRequests(userEnrolment, JourneyType.Accept);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var resultModel = (RedirectToActionResult)result;
            Assert.AreEqual(expected: nameof(ApplicationsController.EnrolmentRequests), actual: resultModel.ActionName);
            Assert.AreEqual(expected: "Applications", actual: resultModel.ControllerName);
        }
        
        [TestMethod]
        public async Task? GivenEnrolmentRequestsPost_WhenRejectJourney_ThenRedirectToRejectApplication_AndSaveSession()
        {
            // Arrange
            var userEnrolment = _fixture.Build<UserEnrolment>()
                .With(x => x.IsApprovedUserAccepted)
                .With(x => x.User)
                .Create();

            userEnrolment.User.FirstName = RejectedUserFirstName;
            userEnrolment.User.LastName = RejectedUserLastName;
            userEnrolment.User.Email = RejectedEmail;
            userEnrolment.User.Enrolment.ServiceRole = ApprovedUserServiceRole;
            userEnrolment.IsApprovedUserAccepted = false;

            // Act
            var result = await _systemUnderTest.EnrolmentRequests(userEnrolment, JourneyType.Reject);
        
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            var resultModel = (RedirectToActionResult)result;
            resultModel.ActionName.Should().BeEquivalentTo(nameof(ApplicationsController.EnrolmentDecision));
            resultModel.ControllerName.Should().BeEquivalentTo("Applications");

            var rejectUserJourneyData = JourneySessionMock.RegulatorSession.RejectUserJourneyData;
            rejectUserJourneyData.FirstName.Should().Be(RejectedUserFirstName);
            rejectUserJourneyData.LastName.Should().Be(RejectedUserLastName);
            rejectUserJourneyData.Email.Should().Be(RejectedEmail);
            rejectUserJourneyData.ServiceRole.Should().Be(ApprovedUserServiceRole);
            rejectUserJourneyData.Decision.Should().Be(RejectedDecision);
        }

        [TestMethod]
        public async Task? GivenGetEnrolmentDecision_AndApprovedPerson_ThenRejectPersonAndReturnEnrolmentDecisionView()
        {
            // Arrange
            string serviceRole = ServiceRole.ApprovedPerson;
            string decision = "Rejected";
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = serviceRole,
                        FirstName = RejectedUserFirstName,
                        LastName = RejectedUserLastName,
                        Email = RejectedEmail,
                        Decision = decision
                    }
                }
            };
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision();
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
        
            var resultView = (ViewResult)result;
            Assert.IsNotNull(resultView);
        
            object? resultModel = resultView.Model;
            Assert.IsNotNull(resultModel);
            resultModel.Should().BeOfType<EnrolmentDecisionViewModel>();
            Assert.AreEqual(expected: session.RegulatorSession.OrganisationId, actual: _acceptedUserOrganisationId);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.ServiceRole, actual: serviceRole);
            Assert.AreEqual(expected: RejectedUserFirstName, actual: session.RegulatorSession.RejectUserJourneyData.FirstName);
            Assert.AreEqual(expected: RejectedUserLastName, actual: session.RegulatorSession.RejectUserJourneyData.LastName);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.Decision, actual: decision);
        }
        
        [TestMethod]
        public async Task? GivenGetEnrolmentDecision_AndNotApprovedPerson_ThenApprovePersonAndReturnEnrolmentDecisionView()
        {
            // Arrange
            string serviceRole = ServiceRole.DelegatedPerson;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = serviceRole,
                        FirstName = RejectedUserFirstName,
                        LastName = RejectedUserLastName,
                        Email = RejectedEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserLastName,
                        Decision = RejectedDecision
                    }
                }
            };
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            var approvedUser = new User()
            {
                FirstName = AcceptedUserFirstName,
                LastName = AcceptedUserLastName,
                Enrolment = new Enrolment()
                {
                    ServiceRole = ServiceRole.ApprovedPerson,
                }
            };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>()))
                .ReturnsAsync(organisationDetails);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision();
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
        
            var resultView = (ViewResult)result;
            Assert.IsNotNull(resultView);
        
            object? resultModel = resultView.Model;
            Assert.IsNotNull(resultModel);
            resultModel.Should().BeOfType<EnrolmentDecisionViewModel>();
            Assert.AreEqual(expected: session.RegulatorSession.OrganisationId, actual: _acceptedUserOrganisationId);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.ServiceRole, actual: serviceRole);
            Assert.AreEqual(expected: AcceptedUserFirstName, actual: approvedUser.FirstName);
            Assert.AreEqual(expected: AcceptedUserLastName, actual: approvedUser.LastName);
            Assert.AreEqual(expected: RejectedDecision, actual: session.RegulatorSession.RejectUserJourneyData.Decision);
        }
        
        [TestMethod]
        [DataRow(null, "tstLast", "tst@testing.com")]
        [DataRow("tstFirst", null, "tst@testing.com")]
        [DataRow("tstFirst", "tstLast", null)]
        [DataRow("tstFirst", "tstLast", "tst@testing.com")]
        public async Task? GivenAcceptApplication_AndGivenNullValues_ThenReturnRedirectToUrl(
            string? firstName, string? lastName, string? email)
        {
            // Arrange
            string role = ServiceRole.ApprovedPerson;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId
                }
            };
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            var approvedUser = new User()
            {
                FirstName = AcceptedUserFirstName,
                LastName = AcceptedUserLastName,
                Enrolment = new Enrolment()
                {
                    ServiceRole = role,
                }
            };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>()))
                .ReturnsAsync(organisationDetails);
        
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            // Act
            var result = await _systemUnderTest.AcceptApplication(firstName, lastName, email, role);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var resultModel = (RedirectToActionResult)result;
            Assert.AreEqual(expected: nameof(ApplicationsController.EnrolmentRequests), actual: resultModel.ActionName);
            Assert.AreEqual(expected: "Applications", actual: resultModel.ControllerName);
        }
        
        [TestMethod]
        [DataRow("tstFirst", "tstLast", "tst@testing.com")]
        public async Task? GivenAcceptApplication_AndGivenNullRole_ThenReturnRedirectToUrl(
            string? firstName, string? lastName, string? email)
        {
            // Arrange
            string role = ServiceRole.ApprovedPerson;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId
                }
            };
        
            var approvedUser = new User()
            {
                FirstName = AcceptedUserFirstName,
                LastName = AcceptedUserLastName,
                Enrolment = new Enrolment()
                {
                    ServiceRole = role,
                }
            };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };
        
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>()))
                .ReturnsAsync(organisationDetails);
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);
        
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
        
            // Act
            var result = await _systemUnderTest.AcceptApplication(firstName, lastName, email, null);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectToActionResult);
            redirectToActionResult.ActionName.Should().Be("EnrolmentRequests");
        }
        
        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_AndGivenInvalidModelStateCommentsRequired_ThenReturnErrorMessage()
        {
            // Arrange
            string role = ServiceRole.ApprovedPerson;
            string decision = RegulatorDecision.Approved;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = role,
                        FirstName = RejectedUserFirstName,
                        LastName = RejectedUserLastName,
                        Email = RejectedEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        Decision = decision
                    },
                }
            };
        
            var enrolmentDecision = new EnrolmentDecisionViewModel();
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, CommentsFieldRequiredErrorMessage);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision(enrolmentDecision);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            var resultView = (ViewResult)result;
            Assert.IsNotNull(resultView);
            Assert.AreEqual(expected: 1, actual: resultView.ViewData.ModelState.ErrorCount);
            Assert.AreEqual(expected: "EnrolmentDecision", actual: resultView.ViewName);
        }
        
        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_AndGivenInvalidModelState_ThenReturnErrorMessage()
        {
            // Arrange
            string role = ServiceRole.ApprovedPerson;
            string decision = RegulatorDecision.Approved;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = role,
                        FirstName = RejectedUserFirstName,
                        LastName = RejectedUserLastName,
                        Email = RejectedEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        Decision = decision
                    }
                }
            };
        
            var enrolmentDecision = new EnrolmentDecisionViewModel();
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, ModelStateErrorMsg);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision(enrolmentDecision);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            var resultView = (ViewResult)result;
            Assert.IsNotNull(resultView);
            Assert.AreEqual(expected: 1, actual: resultView.ViewData.ModelState.ErrorCount);
            Assert.AreEqual(
                expected: "Test Model Status Invalid",
                actual: resultView.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage.Trim());
            Assert.AreEqual(expected: "EnrolmentDecision", actual: resultView.ViewName);
        }
        
        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_WithValidModelStateAndServiceRoleApprovedPerson_ThenReturnRedirectToAction()
        {
            // Arrange
            string role = ServiceRole.ApprovedPerson;
            string decision = RegulatorDecision.Approved;
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = role,
                        FirstName = RejectedUserFirstName,
                        LastName = RejectedUserLastName,
                        Email = RejectedEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        Decision = decision
                    }
                }
            };
        
            var approvedUser = new User()
            {
                FirstName = AcceptedUserFirstName,
                LastName = AcceptedUserFirstName,
                Enrolment = new Enrolment()
                {
                    ServiceRole = role,
                }
            };
            var delegatedUser = new User()
            {
                FirstName = DelegatedUserFirstName,
                LastName = DelegatedUserFirstName,
                Enrolment = new Enrolment()
                {
                    ServiceRole = ServiceRole.DelegatedPerson,
                }
            };
        
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser, delegatedUser},
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };
        
            var enrolmentDecision = new EnrolmentDecisionViewModel();
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>())).ReturnsAsync(organisationDetails);
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>())).ReturnsAsync(EndpointResponseStatus.Success);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision(enrolmentDecision);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectToActionResult);
            Assert.AreEqual(expected: "Applications", actual: redirectToActionResult.ActionName);
        }
        
        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_WithValidModelStateAndServiceRoleNotApprovedPerson_ThenReturnRedirectToAction()
        {
            // Arrange
            string role = ServiceRole.DelegatedPerson;
            string decision = RegulatorDecision.Approved;
            var enrolmentId = Guid.NewGuid();
        
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = role,
                        FirstName = DelegatedUserFirstName,
                        LastName = DelegatedUserFirstName,
                        Email = DelegatedUserEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        Decision = decision
                    }
                }
            };
        
            var approvedUser = new User()
            {
                FirstName = DelegatedUserFirstName,
                LastName = DelegatedUserFirstName,
                Email = DelegatedUserEmail,
                Enrolment = new Enrolment()
                {
                    ServiceRole = role,
                    ExternalId = enrolmentId
                }
            };
            var delegatedUser = new User()
            {
                FirstName = DelegatedUserFirstName,
                LastName = DelegatedUserFirstName,
                Email = DelegatedUserEmail,
                Enrolment = new Enrolment()
                {
                    ServiceRole = ServiceRole.DelegatedPerson,
                }
            };
        
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser, delegatedUser },
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };
        
            var enrolmentDecision = new EnrolmentDecisionViewModel()
            {
               RejectedUserFirstName = RejectedUserFirstName,
               RejectedUserLastName= RejectedUserLastName,
               Comments = "Descision Comment"
            };
        
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>())).ReturnsAsync(organisationDetails);
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>())).ReturnsAsync(EndpointResponseStatus.Success);
        
            // Act
            var result = await _systemUnderTest.EnrolmentDecision(enrolmentDecision);
        
            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var resultRedirect = (RedirectToActionResult)result;
            Assert.IsNotNull(resultRedirect);
            Assert.AreEqual(expected: "EnrolmentRequests", actual: resultRedirect.ActionName);
        }

        [TestMethod]
        public async Task?
            GivenPostEnrolmentDecision_WithValidModelStateAndServiceRoleNotApprovedPersonAndUpdateFails_ThenReturnRedirectToAction()
        {
            // Arrange
            string role = ServiceRole.DelegatedPerson;
            string decision = RegulatorDecision.Approved;
            var enrolmentId = Guid.NewGuid();

            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ServiceRole = role,
                        FirstName = DelegatedUserFirstName,
                        LastName = DelegatedUserFirstName,
                        Email = DelegatedUserEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        Decision = decision
                    }
                }
            };

            var approvedUser = new User()
            {
                FirstName = DelegatedUserFirstName,
                LastName = DelegatedUserFirstName,
                Email = DelegatedUserEmail,
                Enrolment = new Enrolment() { ServiceRole = role, ExternalId = enrolmentId }
            };
            var delegatedUser = new User()
            {
                FirstName = DelegatedUserFirstName,
                LastName = DelegatedUserFirstName,
                Email = DelegatedUserEmail,
                Enrolment = new Enrolment() { ServiceRole = ServiceRole.DelegatedPerson, }
            };

            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser, delegatedUser },
                OrganisationId = _acceptedUserOrganisationId,
                BusinessAddress = new BusinessAddress()
            };

            var enrolmentDecision = new EnrolmentDecisionViewModel()
            {
                RejectedUserFirstName = RejectedUserFirstName,
                RejectedUserLastName = RejectedUserLastName,
                Comments = "Descision Comment"
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>()))
                .ReturnsAsync(organisationDetails);
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .ReturnsAsync(EndpointResponseStatus.Fail);

            // Act
            var result = await _systemUnderTest.EnrolmentDecision(enrolmentDecision);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var resultRedirect = (RedirectToActionResult)result;
            Assert.IsNotNull(resultRedirect);
            Assert.AreEqual(expected: "EnrolmentRequests", actual: resultRedirect.ActionName);
        }
    }
}
