using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using EPR.RegulatorService.Frontend.Web.Controllers.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class ApplicationsControllerTests : ApplicationsTestBase
    {
        private const string TransferredOrganisationName = "Organisation to transfer";
        private const string AgencyToTransferTo = "Some nation";
        private readonly Guid _acceptedUserOrganisationId = Guid.Parse("89fda59e-18e0-47a5-bd20-8ca060a2fa00");
        private const string AcceptedUserFirstName = "King";
        private const string AcceptedUserLastName = "Walter";
        private const string AcceptedUserEmail = "Walter@email.com";
        private const string RejectedUserFirstName = "Barry";
        private const string RejectedUserLastName = "Test";
        private const string RejectedEmail = "btest@test.com";
        private const string DelegatedUserFirstName = "Tom";
        private const string DelegatedUserEmail = "Tom@email.com";
        private const int DefaultPageNumber = 1;
        private const int PageNumberTwo = 2;
        private const string ModelStateErrorMsg = "Test Model Status Invalid";
        private const string CommentsFieldRequiredErrorMessage = "The Comments field is required.";
        private string _searchString = "test";

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            _searchString = "test";

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task Applications_WithNullSession_CreatesNewSessionAndReturnsView()
        {
            // Act
            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel {
                     SearchOrganisationName = _searchString,
                     IsApprovedUserTypeChecked = false,
                     IsDelegatedUserTypeChecked = false,
                     TransferNationResult = EndpointResponseStatus.NotSet,
                     TransferredOrganisationAgency = null,
                     TransferredOrganisationName = null
                });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();

            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");

            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsView()
        {
            // Act
            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = false,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.NotSet,
                    TransferredOrganisationAgency = null,
                    TransferredOrganisationName = null
                });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();
            viewResult.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");

            var viewModel = viewResult.Model as ApplicationsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFiltersApplied_And_IsFilteredSearch()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = true,
                    OrganisationId = Guid.NewGuid()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.NotSet,
                    TransferredOrganisationAgency = null,
                    TransferredOrganisationName = null,
                    ClearFilters = false,
                    IsFilteredSearch = true
                });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();

            var viewModel = viewResult.Model as ApplicationsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFiltersApplied_And_Not_IsFilteredSearch()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = true,
                    OrganisationId = Guid.NewGuid()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = null,
                    IsApprovedUserTypeChecked = false,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.NotSet,
                    TransferredOrganisationAgency = null,
                    TransferredOrganisationName = null
                });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();

            var viewModel = viewResult.Model as ApplicationsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(_searchString);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_SavedFilters_Are_Cleared()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = true,
                    OrganisationId = Guid.NewGuid()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = null,
                    IsApprovedUserTypeChecked = false,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.NotSet,
                    TransferredOrganisationAgency = null,
                    TransferredOrganisationName = null,
                    ClearFilters = true
                });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();

            var viewModel = viewResult.Model as ApplicationsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(DefaultPageNumber);
            viewModel.SearchOrganisationName.Should().Be(string.Empty);
            viewModel.IsApprovedUserTypeChecked.Should().BeFalse();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_RedirectedFromAnotherPage()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    SearchOrganisationName = string.Empty,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = false,
                    OrganisationId = Guid.NewGuid(),
                    CurrentPageNumber = PageNumberTwo
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var result = await _systemUnderTest.Applications(new ApplicationsRequestViewModel
            {
                SearchOrganisationName = null,
                IsApprovedUserTypeChecked = false,
                IsDelegatedUserTypeChecked = false,
                TransferNationResult = EndpointResponseStatus.NotSet,
                TransferredOrganisationAgency = null,
                TransferredOrganisationName = null,
                ClearFilters = false
            });

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            viewResult.Model.Should().BeOfType<ApplicationsViewModel>();

            var viewModel = viewResult.Model as ApplicationsViewModel;
            Assert.IsNotNull(viewModel);
            viewModel.PageNumber.Should().Be(PageNumberTwo);
            viewModel.SearchOrganisationName.Should().Be(string.Empty);
            viewModel.IsApprovedUserTypeChecked.Should().BeTrue();
            viewModel.IsDelegatedUserTypeChecked.Should().BeFalse();
        }

        [TestMethod]
        public async Task Applications_WithValidSession_ReturnsCorrectViewAndModel_Where_Organisation_Is_Transferred()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = true,
                    IsDelegatedUserTypeChecked = true,
                    OrganisationId = Guid.NewGuid()
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act
            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = null,
                    IsApprovedUserTypeChecked = false,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.Success,
                    TransferredOrganisationName = TransferredOrganisationName,
                    TransferredOrganisationAgency = AgencyToTransferTo
                });

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<ApplicationsViewModel>();
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel!.TransferNationResult.Should().Be(EndpointResponseStatus.Success);
            viewModel!.TransferredOrganisationName.Should().Be(TransferredOrganisationName);
            viewModel!.TransferredOrganisationAgency.Should().Be(AgencyToTransferTo);
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

            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson } };
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
            var result = await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail,
                Frontend.Web.Constants.ServiceRole.ApprovedPerson);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = result as RedirectResult;
            redirectResult!.Url.Should().Be($"~/enrolment-requests?organisationId={organisationId}");
        }

        [TestMethod]
        public async Task? Application_Accepted_But_Email_Failed_To_Send_Stay_On_Same_Page()
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

            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson } };
            var organisationDetails = new OrganisationEnrolments()
            {
                Users = new List<User>() { approvedUser },
                OrganisationId = organisationId,
                BusinessAddress = new BusinessAddress()
            };
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(organisationId))
                .ReturnsAsync(organisationDetails);

            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>()))
                .Throws(new Exception("Failed to send email"));

            // Act
            var result = await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail,
                Frontend.Web.Constants.ServiceRole.ApprovedPerson);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.ViewName.Should().Be(nameof(ApplicationsController.EnrolmentRequests));
        }

        [TestMethod]
        public async Task? Application_Accepted_But_Database_Save_Failed_Stay_On_Same_Page()
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

            var approvedUser = new User() { Enrolment = new Enrolment() { ServiceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson } };
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

            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>()))
                .Throws(new Exception("Failed to save to DB"));

            // Act
            var result = await _systemUnderTest.AcceptApplication(AcceptedUserFirstName, AcceptedUserLastName, AcceptedUserEmail,
                Frontend.Web.Constants.ServiceRole.ApprovedPerson);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.ViewName.Should().Be(nameof(ApplicationsController.EnrolmentRequests));
        }

        [TestMethod]
        public async Task? GivenPreEnrolmentDecision_WhhenValidData_ThenSaveToSessionAndRedirectToApplications()
        {
            // Arrange
            string serviceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson;
            string decision = "Rejected";

            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.PreEnrolmentDecision(RejectedUserFirstName, RejectedUserLastName, RejectedEmail, serviceRole, decision);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            var resultModel = (RedirectToActionResult)result;
            Assert.AreEqual(expected: nameof(ApplicationsController.EnrolmentDecision), actual: resultModel.ActionName);
            Assert.AreEqual(expected: "Applications", actual: resultModel.ControllerName);
        }

        [TestMethod]
        public async Task? GivenGetEnrolmentDecision_AndApprovedPerson_ThenRejectPersonAndReturnEnrolmentDecisionView()
        {
            // Arrange
            string serviceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson;
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
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.FirstName, actual: RejectedUserFirstName);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.LastName, actual: RejectedUserLastName);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.Decision, actual: decision);
        }

        [TestMethod]
        public async Task? GivenGetEnrolmentDecision_AndNotApprovedPerson_ThenApprovePersonAndReturnEnrolmentDecisionView()
        {
            // Arrange
            string serviceRole = Frontend.Web.Constants.ServiceRole.DelegatedPerson;
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
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserLastName,
                        Decision = decision
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
                    ServiceRole = Frontend.Web.Constants.ServiceRole.ApprovedPerson,
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
            Assert.AreEqual(expected: approvedUser.FirstName, actual: AcceptedUserFirstName);
            Assert.AreEqual(expected: approvedUser.LastName, actual: AcceptedUserLastName);
            Assert.AreEqual(expected: session.RegulatorSession.RejectUserJourneyData.Decision, actual: decision);
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
            string role = Frontend.Web.Constants.ServiceRole.ApprovedPerson;

            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserLastName,
                        Email = AcceptedUserEmail
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
            result.Should().BeOfType<RedirectResult>();
            var resultRedirect = (RedirectResult)result;
            Assert.IsNotNull(resultRedirect);
            object? resultModel = resultRedirect.Url;
            Assert.AreEqual(expected: $"~/enrolment-requests?organisationId={_acceptedUserOrganisationId}", actual: resultRedirect.Url);
        }

        [TestMethod]
        [DataRow("tstFirst", "tstLast", "tst@testing.com")]
        public async Task? GivenAcceptApplication_AndGivenNullRole_ThenReturnRedirectToUrl(
            string? firstName, string? lastName, string? email)
        {
            // Arrange
            string role = Frontend.Web.Constants.ServiceRole.ApprovedPerson;

            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserLastName,
                        Email = AcceptedUserEmail
                    }
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
            result.Should().BeOfType<RedirectResult>();
            var resultRedirect = (RedirectResult)result;
            Assert.IsNotNull(resultRedirect);
            object? resultModel = resultRedirect.Url;
            Assert.AreEqual(expected: $"~/enrolment-requests?organisationId={_acceptedUserOrganisationId}", actual: resultRedirect.Url);
        }

        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_AndGivenInvalidModelStateCommentsRequired_ThenReturnErrorMessage()
        {
            // Arrange
            string role = Frontend.Web.Constants.ServiceRole.ApprovedPerson;
            string decision = Frontend.Web.Constants.RegulatorDecision.Approved;

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
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
                    }
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
            string role = Frontend.Web.Constants.ServiceRole.ApprovedPerson;
            string decision = Frontend.Web.Constants.RegulatorDecision.Approved;

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
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
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
            string role = Frontend.Web.Constants.ServiceRole.ApprovedPerson;
            string decision = Frontend.Web.Constants.RegulatorDecision.Approved;

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
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
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
                    ServiceRole = Frontend.Web.Constants.ServiceRole.DelegatedPerson,
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
            var resultRedirect = (RedirectToActionResult)result;
            Assert.IsNotNull(resultRedirect);
            Assert.AreEqual(expected: "Applications", actual: resultRedirect.ActionName);
        }

        [TestMethod]
        public async Task? GivenPostEnrolmentDecision_WithValidModelStateAndServiceRoleNotApprovedPerson_ThenReturnRedirectToAction()
        {
            // Arrange
            string role = Frontend.Web.Constants.ServiceRole.DelegatedPerson;
            string decision = Frontend.Web.Constants.RegulatorDecision.Approved;
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
                    },
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
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
                    ServiceRole = Frontend.Web.Constants.ServiceRole.DelegatedPerson,
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
        public async Task? GivenPostEnrolmentDecision_WithValidModelStateAndServiceRoleNotApprovedPersonAndUpdateFails_ThenReturnRedirectToAction()
        {
            // Arrange
            string role = Frontend.Web.Constants.ServiceRole.DelegatedPerson;
            string decision = Frontend.Web.Constants.RegulatorDecision.Approved;
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
                    },
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        ServiceRole = role,
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
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
                    ServiceRole = Frontend.Web.Constants.ServiceRole.DelegatedPerson,
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
                RejectedUserLastName = RejectedUserLastName,
                Comments = "Descision Comment"
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            _facadeServiceMock.Setup(x => x.GetOrganisationEnrolments(It.IsAny<Guid>())).ReturnsAsync(organisationDetails);
            _facadeServiceMock.Setup(x => x.SendEnrolmentEmails(It.IsAny<EnrolmentDecisionRequest>())).ReturnsAsync(EndpointResponseStatus.Success);
            _facadeServiceMock.Setup(x => x.UpdateEnrolment(It.IsAny<UpdateEnrolment>())).ReturnsAsync(EndpointResponseStatus.Fail);

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
        public async Task Applications_WithNullSessionAndPageNumber_CreatesNewSessionAndReturnsView()
        {
            // Arrange
            var session = new JourneySession
            {
                RegulatorSession = new RegulatorSession()
                {
                    OrganisationId = _acceptedUserOrganisationId,
                    RejectUserJourneyData = new RejectUserJourneyData
                    {
                        ResponseStatus = EndpointResponseStatus.Success,
                        FirstName = DelegatedUserFirstName,
                        LastName = DelegatedUserFirstName,
                        Email = DelegatedUserEmail,
                        ApprovedUserFirstName = AcceptedUserFirstName,
                        ApprovedUserLastName = AcceptedUserFirstName,
                        OrganisationId = Guid.NewGuid()
                    },
                    AcceptUserJourneyData = new AcceptUserJourneyData()
                    {
                        FirstName = AcceptedUserFirstName,
                        LastName = AcceptedUserFirstName,
                        Email = AcceptedUserEmail
                    }
                }
            };
            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await _systemUnderTest.Applications(
                new ApplicationsRequestViewModel
                {
                    SearchOrganisationName = _searchString,
                    IsApprovedUserTypeChecked = false,
                    IsDelegatedUserTypeChecked = false,
                    TransferNationResult = EndpointResponseStatus.NotSet,
                    TransferredOrganisationAgency = null,
                    TransferredOrganisationName = null,
                    ClearFilters = false,
                    IsFilteredSearch = false,
                    PageNumber = 1
                });

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<ApplicationsViewModel>();
            var viewModel = viewResult.Model as ApplicationsViewModel;
            viewModel!.PageNumber.Should().Be(DefaultPageNumber);
            viewModel!.SearchOrganisationName.Should().Be(_searchString);
            viewResult!.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
            viewResult!.ViewData["CustomBackLinkToDisplay"].Should().Be($"/regulators/{PagePath.Home}");
        }
    }
}
