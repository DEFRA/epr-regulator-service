using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.ViewModels.Registrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    [TestClass]
    public class RegistrationControllerTests : RegistrationTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();

        }

        [TestMethod]
        public async Task RegistrationsGet_CreatesNewSessionAndReturnsView()
        {
            // Act
            var result = await _sut.Registrations(null, null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

            var journeySession = await _sessionManagerMock.Object.GetSessionAsync(It.IsAny<ISession>());
            journeySession.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RegistrationsGet_NewPageNumber_UpdatesCurrentPage()
        {
            // Act
            var result = await _sut.Registrations(5, null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var registrationsViewModel = viewResult.Model as RegistrationsViewModel;
            registrationsViewModel.PageNumber.Should().Be(5);
        }

        [TestMethod]
        public async Task RegistrationsPost_WithNullSession_CreatesNewSessionAndReturnsView()
        {
            // Act
            var result = await _sut.Registrations(new RegistrationFiltersModel());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();

            var viewResult = result as RedirectToActionResult;
            viewResult.Should().NotBeNull();
            viewResult.RouteValues.Should().BeNull();

            var journeySession = await _sessionManagerMock.Object.GetSessionAsync(It.IsAny<ISession>());
            journeySession.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RegistrationsPost_WithValidSession_ReturnsCorrectViewAndModel_Where_SelectedFiltersOverwriteSessionFilters_And_IsFilteredSearch()
        {
            // Arrange
            var registrationFiltersModel = GetRegistrationFiltersModel();
            SetupJourneySession(registrationFiltersModel);

            // Act
            var result = await _sut.Registrations(registrationFiltersModel);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();

            var viewResult = result as RedirectToActionResult;
            viewResult.Should().NotBeNull();
            viewResult.RouteValues.Should().BeNull();

            var journeySession = await _sessionManagerMock.Object.GetSessionAsync(It.IsAny<ISession>());
            var registrationFiltersModelFromSession = journeySession.RegulatorRegistrationSession.RegistrationFiltersModel;
            registrationFiltersModelFromSession.SearchOrganisationName.Should().Be("Test Organisation");
            registrationFiltersModelFromSession.SearchOrganisationId.Should().Be("123");
            registrationFiltersModelFromSession.IsDirectProducerChecked.Should().BeTrue();
            registrationFiltersModelFromSession.IsComplianceSchemeChecked.Should().BeTrue();
            registrationFiltersModelFromSession.IsPendingRegistrationChecked.Should().BeTrue();
            registrationFiltersModelFromSession.IsAcceptedRegistrationChecked.Should().BeTrue();
            registrationFiltersModelFromSession.IsRejectedRegistrationChecked.Should().BeTrue();
        }

        [TestMethod]
        public async Task RegistrationsPost_WithValidSession_ReturnsCorrectViewAndModel_Where_SessionFiltersApplied_And_Not_IsFilteredSearch()
        {
            // Arrange
            SetupJourneySession(new RegistrationFiltersModel());

            var result = await _sut.Registrations(new RegistrationFiltersModel());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();

            var viewResult = result as RedirectToActionResult;
            viewResult.Should().NotBeNull();
            viewResult.RouteValues.Should().BeNull();

            var journeySession = await _sessionManagerMock.Object.GetSessionAsync(It.IsAny<ISession>());
            var registrationFiltersModel = journeySession.RegulatorRegistrationSession.RegistrationFiltersModel;
            registrationFiltersModel.SearchOrganisationName.Should().BeEmpty();
            registrationFiltersModel.SearchOrganisationId.Should().BeEmpty();
            registrationFiltersModel.IsDirectProducerChecked.Should().BeFalse();
            registrationFiltersModel.IsComplianceSchemeChecked.Should().BeFalse();
            registrationFiltersModel.IsPendingRegistrationChecked.Should().BeFalse();
            registrationFiltersModel.IsAcceptedRegistrationChecked.Should().BeFalse();
            registrationFiltersModel.IsRejectedRegistrationChecked.Should().BeFalse();
        }
    }
}