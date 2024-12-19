namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers;

using AutoFixture;

using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;

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
    public async Task RegistrationsGet_ExportTrue_ReturnsCsvFile()
    {
        // Arrange
        var data = new byte[1] { 1 };

        _facadeServiceMock.Setup(x => x.GetRegistrationSubmissionsCsv(It.IsAny<GetRegistrationSubmissionsCsvRequest>()))
            .ReturnsAsync(new MemoryStream(data));

        // Act

        var result = await _sut.Registrations(new RegistrationFiltersModel(), null, false, null, null, null, true);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<FileStreamResult>();

        var fileStreamResult = result as FileStreamResult;
        fileStreamResult.ContentType.Should().Be("text/csv");
        fileStreamResult.FileDownloadName.Should().Be("registration-submissions.csv");
        fileStreamResult.FileStream.ReadByte().Should().Be(data[0]);
        fileStreamResult.FileStream.ReadByte().Should().Be(-1);
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
        registrationFiltersModelFromSession.SearchSubmissionYears.Should().BeEquivalentTo(new[] { 2023 });
        registrationFiltersModelFromSession.SearchSubmissionPeriods.Should().BeEquivalentTo("January to June 2023");
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
        registrationFiltersModel.SearchSubmissionYears.Should().BeEmpty();
        registrationFiltersModel.SearchSubmissionPeriods.Should().BeEmpty();
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
        registrationFiltersModel.SearchSubmissionYears.Should().BeEmpty();
        registrationFiltersModel.SearchSubmissionPeriods.Should().BeEmpty();
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
        model.PartnershipDetailsFileId.Should().Be(registration.PartnershipFileId);
        model.PartnershipDetailsFileName.Should().Be(registration.PartnershipFileName);
        model.BrandDetailsFileId.Should().Be(registration.BrandsFileId);
        model.BrandDetailsFileName.Should().Be(registration.BrandsFileName);
        model.DeclaredBy.Should().Be(registration.OrganisationType == OrganisationType.ComplianceScheme
            ? "Not required (compliance scheme)"
            : $"{registration.FirstName} {registration.LastName}");
    }

    [TestMethod]
    public void OrganisationDetailsFileDownload_ReturnsViewWithViewModel()
    {
        // Act
        var result = _sut.OrganisationDetailsFileDownload();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task FileDownload_RedirectsToOrganisationDetailsFileDownload()
    {
        // Arrange
        var session = _fixture.Create<JourneySession>();
        session.RegulatorRegistrationSession = new RegulatorRegistrationSession
        {
            FileDownloadRequestType = FileDownloadTypes.OrganisationDetails.ToString()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.FileDownload(FileDownloadTypes.OrganisationDetails.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("OrganisationDetailsFileDownload");
        redirectResult.ControllerName.Should().Be("Registrations");
    }

    [TestMethod]
    public async Task FileDownload_RedirectsToBrandDetailsFileDownload()
    {
        // Arrange
        var session = _fixture.Create<JourneySession>();
        session.RegulatorRegistrationSession = new RegulatorRegistrationSession
        {
            FileDownloadRequestType = FileDownloadTypes.BrandDetails.ToString()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.FileDownload(FileDownloadTypes.OrganisationDetails.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("OrganisationDetailsFileDownload");
        redirectResult.ControllerName.Should().Be("Registrations");
    }

    [TestMethod]
    public async Task FileDownload_RedirectsToPartnershipDetailsFileDownload()
    {
        // Arrange
        var session = _fixture.Create<JourneySession>();
        session.RegulatorRegistrationSession = new RegulatorRegistrationSession
        {
            FileDownloadRequestType = FileDownloadTypes.PartnershipDetails.ToString()
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
        _sessionManagerMock.Setup(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()))
                           .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.FileDownload(FileDownloadTypes.OrganisationDetails.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("OrganisationDetailsFileDownload");
        redirectResult.ControllerName.Should().Be("Registrations");
    }

    [TestMethod]
    public async Task FileDownloadInProgress_ValidRequest_ReturnsFile()
    {
        // Arrange
        var registration = _fixture.Create<Registration>();
        var session = new JourneySession
        {
            RegulatorRegistrationSession = new RegulatorRegistrationSession
            {
                FileDownloadRequestType = FileDownloadTypes.OrganisationDetails.ToString(),
                OrganisationRegistration = registration
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
            var result = await _sut.FileDownloadInProgress();

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
        var registration = _fixture.Create<Registration>();
        var session = new JourneySession
        {
            RegulatorRegistrationSession = new RegulatorRegistrationSession
            {
                FileDownloadRequestType = FileDownloadTypes.OrganisationDetails.ToString(),
                OrganisationRegistration = registration
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);

        _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

        // Act
        var result = await _sut.FileDownloadInProgress();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("OrganisationDetailsFileDownloadSecurityWarning");
        redirectResult.ControllerName.Should().BeNull();
    }

    [TestMethod]
    public async Task FileDownloadInProgress_DownloadFailed_ReturnsRedirectToAction()
    {
        // Arrange
        var registration = _fixture.Create<Registration>();
        var session = new JourneySession
        {
            RegulatorRegistrationSession = new RegulatorRegistrationSession
            {
                FileDownloadRequestType = FileDownloadTypes.OrganisationDetails.ToString(),
                OrganisationRegistration = registration
            }
        };

        _sessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

        using (var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest))
        {
            _facadeServiceMock.Setup(x => x.GetFileDownload(It.IsAny<FileDownloadRequest>())).ReturnsAsync(response);

            // Act
            var result = await _sut.FileDownloadInProgress();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            var redirectResult = result as RedirectToActionResult;
            redirectResult.ActionName.Should().Be("OrganisationDetailsFileDownloadFailed");
            redirectResult.ControllerName.Should().BeNull();
        }
    }

    [TestMethod]
    public void OrganisationDetailsFileDownloadFailed_ReturnsViewWithViewModel()
    {
        // Act
        var result = _sut.OrganisationDetailsFileDownloadFailed();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<OrganisationDetailsFileDownloadViewModel>();
        var model = (OrganisationDetailsFileDownloadViewModel)viewResult.Model;
        model.DownloadFailed.Should().BeTrue();
        model.HasIssue.Should().BeFalse();
    }

    [TestMethod]
    public void OrganisationDetailsFileDownloadSecurityWarning_ReturnsViewWithViewModel()
    {
        // Act
        var result = _sut.OrganisationDetailsFileDownloadSecurityWarning();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult.Model.Should().NotBeNull();
        viewResult.Model.Should().BeOfType<OrganisationDetailsFileDownloadViewModel>();
        var model = (OrganisationDetailsFileDownloadViewModel)viewResult.Model;
        model.DownloadFailed.Should().BeTrue();
        model.HasIssue.Should().BeTrue();
    }
}