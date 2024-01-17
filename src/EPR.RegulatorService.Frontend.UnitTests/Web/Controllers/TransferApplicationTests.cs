using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.Applications;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using EPR.RegulatorService.Frontend.UnitTests.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class TransferApplicationTests : ApplicationsTestBase
    {
        private const string ViewName = "TransferApplication";

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
                        PagePath.TransferApplication
                    },
                }
            };

            _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);
        }
        
        [TestMethod]
        public async Task GivenOnTransferApplicationsPage_WhenTransferApplicationHttpGetCalled_ThenAssertBackLinkSet_And_Set_Session()
        {
            // Arrange
            JourneySessionMock!.RegulatorSession.OrganisationName = TransferDetails.OrganisationName;
            
            // Act
            var result = await _systemUnderTest.TransferApplication() as ViewResult;
            

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewName);
            result.ViewName.Should().Be(ViewName);
            
            var model = result.Model as TransferApplicationViewModel;
            model!.OrganisationName.Should().Be(TransferDetails.OrganisationName);
            AssertBackLink(result, PagePath.EnrolmentRequests);
        }
        
        [TestMethod]
        public async Task GivenOnTransferApplicationsPage_WhenTransferApplicationHttpPostCalled_NoNationSelected_ThenThrowValidationError()
        {
            // Act
            var viewModel = new TransferApplicationViewModel();
            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, TransferDetails.ModelErrorValueNoAgencyIndexSelected);

            var result = await _systemUnderTest.TransferApplication(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewName);
            result.ViewName.Should().Be(ViewName);

            Assert.AreEqual(
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: TransferDetails.ModelErrorValueNoAgencyIndexSelected
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }
        
        [TestMethod]
        public async Task GivenOnTransferApplicationsPage_WhenTransferApplicationHttpPostCalled_NoTransferDetailsEntered_ThenThrowValidationError()
        {
            // Act
            var viewModel = new TransferApplicationViewModel()
            {
                AgencyIndex = 0
            };
            
            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, TransferDetails.ModelErrorValueNoTransferDetails);

            var result = await _systemUnderTest.TransferApplication(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewName);
            result.ViewName.Should().Be(ViewName);

            Assert.AreEqual(
                expected: result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: TransferDetails.ModelErrorValueNoTransferDetails
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }
        
        [TestMethod]
        public async Task GivenOnTransferApplicationsPage_WhenTransferApplicationHttpPostCalled_TransferDetailsTooLong_ThenThrowValidationError()
        {
            // Act
            var viewModel = new TransferApplicationViewModel()
            {
                AgencyIndex = 0,
                TransferNotes = new List<TransferNote>
                {
                    new() { AgencyName = TransferDetails.ScotlandAgencyName, Notes = TransferDetails.LongTransferDetailsString, NationId = TransferDetails.SelectedNationIdScotland},
                    new() { AgencyName = TransferDetails.EnglandAgencyName, Notes = string.Empty, NationId = TransferDetails.SelectedNationIdEngland},
                    new() { AgencyName = TransferDetails.NorthernIrelandAgencyName, Notes = TransferDetails.TransferDetailsString, NationId = TransferDetails.SelectedNationIdNorthernIreland},
                    new() { AgencyName = TransferDetails.WalesAgencyName, Notes = string.Empty, NationId = TransferDetails.SelectedNationIdWales}
                }
            };
            
            _systemUnderTest.ModelState.AddModelError(ModelErrorKey, TransferDetails.ModelErrorValueSummaryTooLong);

            var result = await _systemUnderTest.TransferApplication(viewModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ViewName);
            result.ViewName.Should().Be(ViewName);
            Assert.AreEqual(expected:
                result.ViewData.ModelState["Error"]!.Errors[0].ErrorMessage,
                actual: TransferDetails.ModelErrorValueSummaryTooLong
            );
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }
        
        [TestMethod]
        public async Task GivenOnTransferApplicationsPage_WhenTransferApplicationHttpPostCalled_ThenRedirectToApplicationsPage_AndUpdateSession()
        {
            // Arrange
            JourneySessionMock!.RegulatorSession.OrganisationId = Guid.NewGuid();
            JourneySessionMock.RegulatorSession.OrganisationName = TransferDetails.OrganisationName;
            
            _facadeServiceMock.Setup(e => e.TransferOrganisationNation(It.IsAny<OrganisationTransferNationRequest>()))
                .ReturnsAsync(EndpointResponseStatus.Success);
            
            var viewModel = new TransferApplicationViewModel()
            {
                AgencyIndex = 0,
                TransferNotes = new List<TransferNote>()
                {
                    new() { AgencyIndex = 0, AgencyName = TransferDetails.ScotlandAgencyName, Notes = TransferDetails.TransferDetailsString, NationId = TransferDetails.SelectedNationIdScotland },
                    new() { AgencyIndex = 1, AgencyName = TransferDetails.EnglandAgencyName, Notes = TransferDetails.LongTransferDetailsString, NationId = TransferDetails.SelectedNationIdEngland },
                    new() { AgencyIndex = 2, AgencyName = TransferDetails.NorthernIrelandAgencyName, Notes = string.Empty, NationId = TransferDetails.SelectedNationIdNorthernIreland },
                    new() { AgencyIndex = 3, AgencyName = TransferDetails.WalesAgencyName, Notes = string.Empty, NationId = TransferDetails.SelectedNationIdWales }
                },
                OrganisationName = TransferDetails.OrganisationName
            };
            
            // Act
            var result = await _systemUnderTest.TransferApplication(viewModel) as RedirectToActionResult;
            
            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().NotBeNull();
            result.ActionName.Should().Be(nameof(ApplicationsController.Applications));
            
            _systemUnderTest.TempData["TransferNationResult"].Should().Be(EndpointResponseStatus.Success);
            _systemUnderTest.TempData["TransferredOrganisationName"].Should().Be(TransferDetails.OrganisationName);
            _systemUnderTest.TempData["TransferredOrganisationAgency"].Should().Be(TransferDetails.ScotlandAgencyName);
            _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }
    }
}

