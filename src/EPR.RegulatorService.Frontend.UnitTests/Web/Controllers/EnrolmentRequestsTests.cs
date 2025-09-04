using EPR.RegulatorService.Frontend.Core.MockedData;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using ServiceRole = Frontend.Web.Constants.ServiceRole;

    [TestClass]
    public class EnrolmentRequestsTests : ApplicationsTestBase
    {
        private const string ViewName = "EnrolmentRequests";
        private const string ApprovedUserEmail = "jtester@test.com";
        private const string DelegatedUserOneEmail = "ptester@test.com";
        private const string DelegatedUserTwoEmail = "atester@test.com";
        private readonly Guid _organisationId = Guid.NewGuid();

        [TestInitialize]
        public void Setup()
        {
            SetupBase();
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    Journey = new List<string>
                     {
                         PagePath.Applications,
                         PagePath.EnrolmentRequests,
                     },
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyDataIsNull_ThenAssertBackLinkSet()
        {
            // Arrange
            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            JourneySessionMock.RegulatorSession.OrganisationId = _organisationId;

            // Act
            var result = await _systemUnderTest.EnrolmentRequests() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            AssertBackLink(result, PagePath.Applications);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalled_ThenAssertModelSet()
        {
            // Arrange
            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            JourneySessionMock.RegulatorSession.OrganisationId = _organisationId;

            // Act           
            var result = await _systemUnderTest.EnrolmentRequests();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var view = result as ViewResult;
            view!.ViewName.Should().Be(ViewName);
            view.Model.Should().BeOfType<EnrolmentRequestsViewModel>();

            var model = view.Model as EnrolmentRequestsViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(expected: true, actual: model.IsApprovedUserAccepted);
            Assert.IsNotNull(model.ApprovedUser);
            Assert.AreEqual(expected: ApprovedUserEmail, actual: model.ApprovedUser.Email);
            Assert.AreEqual(expected: DelegatedUserOneEmail, actual: model.DelegatedUsers[0].Email);
            Assert.AreEqual(expected: DelegatedUserTwoEmail, actual: model.DelegatedUsers[1].Email);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyIsNotNull_ThenAssertBackLinkSet()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    Journey = new List<string>
                     {
                         PagePath.Applications,
                         PagePath.EnrolmentRequests,
                     },
                    OrganisationId = _organisationId
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                 .ReturnsAsync(JourneySessionMock);

            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            // Act
            var result = await _systemUnderTest.EnrolmentRequests();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view!.ViewName.Should().Be(ViewName);
            AssertBackLink((ViewResult)result, PagePath.Applications);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyDataOrganisationIdIsNotNull_ThenAssertBackLinkSet()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    Journey = new List<string>
                    {
                        PagePath.Applications,
                        PagePath.EnrolmentRequests,
                    },
                    OrganisationId = _organisationId
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                 .ReturnsAsync(JourneySessionMock);

            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            // Act
            var result = await _systemUnderTest.EnrolmentRequests();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view!.ViewName.Should().Be(ViewName);
            AssertBackLink((ViewResult)result, PagePath.Applications);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyDataOrganisationIdAndResponseStatusIsNull_ThenAssertBackLinkSet()
        {
            // Arrange
            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    Journey = new List<string>
                    {
                        PagePath.Applications,
                        PagePath.EnrolmentRequests,
                    },
                    OrganisationId = _organisationId
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                 .ReturnsAsync(JourneySessionMock);

            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            // Act
            var result = await _systemUnderTest.EnrolmentRequests();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view!.ViewName.Should().Be(ViewName);
            AssertBackLink((ViewResult)result, PagePath.Applications);
        }


        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyDataOrganisationIdAndResponseStatusIsSuccess_ThenAssertBackLinkSet()
        {
            // Arrange
            string firstName = "An";
            string lastName = "Other";

            JourneySessionMock = new JourneySession()
            {
                RegulatorSession = new RegulatorSession()
                {
                    Journey = new List<string>
                    {
                        PagePath.Applications,
                        PagePath.EnrolmentRequests,
                    },
                    OrganisationId = _organisationId
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                 .ReturnsAsync(JourneySessionMock);

            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            _systemUnderTest.TempData["AcceptResult"] = EndpointResponseStatus.Success;
            _systemUnderTest.TempData["AcceptFirstName"] = firstName;
            _systemUnderTest.TempData["AcceptLastName"] = lastName;
            _systemUnderTest.TempData["AcceptedRole"] = ServiceRole.ApprovedPerson;
            
            // Act
            var result = await _systemUnderTest.EnrolmentRequests();

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();

            var view = result as ViewResult;
            view!.ViewName.Should().Be(ViewName);
            AssertBackLink(view, PagePath.Applications);
            view.Model.Should().BeOfType<EnrolmentRequestsViewModel>();
            
            var model = view.Model as EnrolmentRequestsViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(expected: firstName, actual: model.AcceptedFirstName);
            Assert.AreEqual(expected: lastName, actual: model.AcceptedLastName);
            Assert.AreEqual(expected: ServiceRole.ApprovedPerson, actual: model.AcceptedRole);
            Assert.AreEqual(expected: EndpointResponseStatus.Success, actual: model.AcceptStatus);
        }

        [TestMethod]
        public async Task GivenOnEnrolmentRequestsPage_WhenEnrolmentRequestsHttpGetCalledAndAcceptUserJourneyDataIsNullAndTransferDetails_ThenAssertBackLinkSet()
        {
            // Arrange
            var mockedEnrolmentRequestDetails = MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails();
            mockedEnrolmentRequestDetails.TransferDetails = new TransferDetailsResult() { OldNationId = 100, TransferredDate = DateTimeOffset.Now };

            _facadeServiceMock.Setup(e => e.GetOrganisationEnrolments(_organisationId))
                .ReturnsAsync(MockedEnrolmentRequestDetails.GetMockedEnrolmentRequestDetails);

            JourneySessionMock.RegulatorSession.OrganisationId = _organisationId;

            // Act
            var result = await _systemUnderTest.EnrolmentRequests() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.ViewName.Should().Be(ViewName);
            AssertBackLink(result, PagePath.Applications);
        }
    }
}