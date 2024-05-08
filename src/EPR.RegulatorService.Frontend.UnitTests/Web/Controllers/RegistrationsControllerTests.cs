namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers
{
    using AutoFixture;
    using Frontend.Core.Enums;
    using Frontend.Core.Extensions;
    using Frontend.Core.Models.Registrations;
    using Frontend.Web.ViewModels.Registrations;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;

    [TestClass]
    public class RegistrationsControllerTests : RegistrationTestBase
    {
        private Fixture _fixture;

        [TestInitialize]
        public void Setup()
        {
            _fixture = new Fixture();
            SetupBase();
        }

        [TestMethod]
        public async Task RegistrationsGet_CreatesNewSessionAndReturnsView()
        {
            // Act
            var result = await _sut.Registrations(new RegistrationFiltersModel(), null, false);

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
            var result = await _sut.Registrations(new RegistrationFiltersModel(), 5, false);

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
            var result = await _sut.Registrations(new RegistrationFiltersModel(), 1, false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

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
            var result = await _sut.Registrations(registrationFiltersModel, 1, false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

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

            var result = await _sut.Registrations(new RegistrationFiltersModel(), 1, false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

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

        [TestMethod]
        public async Task RegistrationsPost_WithClearFiltersTrue_ReturnsClearedFilter()
        {
            // Arrange
            SetupJourneySession(GetRegistrationFiltersModel());

            var result = await _sut.Registrations(new RegistrationFiltersModel(), 1, true);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();

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

        [TestMethod]
        public async Task Submissions_WithNullSession_CreatesNewSessionAndRedirectsToSubmissionDetails()
        {
            // Act
            var result = await _sut.Registrations("{\r\n  \"SubmissionId\": \"a3f6c7b8-9d4e-4f9a-bcde-1234567890ab\",\r\n  \"SubmissionDate\": \"2023-10-30T10:51:23Z\",\r\n  \"SubmissionStatus\": \"Pending\",\r\n  \"IsResubmission\": false,\r\n  \"IsResubmissionRequired\": false,\r\n  \"RejectionComments\": \"\",\r\n  \"Organisation\": {\r\n    \"OrganisationId\": \"d4e5f6a7-b8c9-0d1e-2f3a-9876543210cd\",\r\n    \"OrganisationName\": \"Acme Inc.\",\r\n    \"OrganisationType\": \"Private\"\r\n  }\r\n}\r\n");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            actionResult?.ControllerName.Should().Be("Registrations");
            actionResult?.ActionName.Should().Be("RegistrationDetails");
        }

        [TestMethod]
        public async Task RegistrationDetails_ReturnsCorrectViewModel()
        {
            // Arrange
            var registration = _fixture.Create<Registration>();
            SetupJourneySession(GetRegistrationFiltersModel(), registration);

            // Act
            var result = await _sut.RegistrationDetails();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult.Model.Should().BeOfType<RegistrationDetailsViewModel>();
            var model = viewResult.Model as RegistrationDetailsViewModel;

            model.OrganisationName.Should().Be(registration.OrganisationName);
            model.BuildingName.Should().Be(registration.BuildingName);
            model.SubBuildingName.Should().Be(registration.SubBuildingName);
            model.BuildingNumber.Should().Be(registration.BuildingNumber);
            model.Street.Should().Be(registration.Street);
            model.Locality.Should().Be(registration.Locality);
            model.DependantLocality.Should().Be(registration.DependantLocality);
            model.Town.Should().Be(registration.Town);
            model.County.Should().Be(registration.County);
            model.Country.Should().Be(registration.Country);
            model.PostCode.Should().Be(registration.PostCode);
            model.OrganisationType.Should().Be(registration.OrganisationType.GetDescription());
            model.OrganisationReferenceNumber.Should().Be(registration.OrganisationReference);
            model.SubmissionId.Should().Be(registration.SubmissionId);
            model.SubmissionPeriod.Should().Be(registration.SubmissionPeriod);
            model.SubmittedBy.Should().Be($"{registration.FirstName} {registration.LastName}");
            model.AccountRole.Should().Be(registration.ServiceRole);
            model.Telephone.Should().Be(registration.Telephone);
            model.Email.Should().Be(registration.Email);
            model.Status.Should().Be(registration.Decision);
            model.IsResubmission.Should().Be(registration.IsResubmission);
            model.RejectionReason.Should().Be(registration.RejectionComments);
            model.PreviousRejectionReason.Should().Be(registration.PreviousRejectionComments);
            model.CompaniesHouseNumber.Should().Be(registration.CompaniesHouseNumber);
            model.OrganisationDetailsFileId.Should().Be(registration.OrganisationDetailsFileId);
            model.OrganisationDetailsFileName.Should().Be(registration.OrganisationDetailsFileName);
            model.PartnershipDetailsFileId.Should().Be(registration.PartnershipDetailsFileId);
            model.PartnershipDetailsFileName.Should().Be(registration.PartnershipDetailsFileName);
            model.BrandDetailsFileId.Should().Be(registration.BrandDetailsFileId);
            model.BrandDetailsFileName.Should().Be(registration.BrandDetailsFileName);
            model.DeclaredBy.Should().Be(registration.OrganisationType == OrganisationType.ComplianceScheme
                ? "Not required (compliance scheme)"
                : $"{registration.FirstName} {registration.LastName}");
        }
    }
}