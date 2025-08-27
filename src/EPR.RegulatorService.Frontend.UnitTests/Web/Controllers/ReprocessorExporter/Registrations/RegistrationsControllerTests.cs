using System.Net;
using System.Text;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

using FluentValidation;

[TestClass]
public class RegistrationsControllerTests : RegistrationControllerTestBase
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private readonly Guid _registrationIdUrlValue = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");

    private readonly string _manageRegistrationUrl = $"manage-registrations?id={Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163").ToString()}";

    private RegistrationsController _controller;
    
    [TestInitialize]
    public void TestInitialize()
    {
        CreateCommonMocks();
        CreateSessionMock();

        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();

        // Set up the Referer header to return a sample URL (or null for different tests)
        mockHeaders.Setup(h => h.Referer).Returns("http://previous-page.com");

        // Set the mock Request to the HttpContext
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);
        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);
        
        var configurationMock = CreateConfigurationMock();
        var validatorMock = new Mock<IValidator<IdRequest>>();
        
        _controller = new RegistrationsController(_sessionManagerMock.Object, _reprocessorExporterServiceMock.Object, configurationMock.Object, _mapperMock.Object, validatorMock.Object);

        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task BusinessAddress_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.BusinessAddress(_registrationIdUrlValue);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task BusinessAddress_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteBusinessAddress(registrationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task BusinessAddress_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.BusinessAddress(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task UkSiteDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");

        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.UkSiteDetails(registrationId);
        });
    }

    [TestMethod]
    public async Task UkSiteDetails_ReturnView()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.UkSiteDetails);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var siteDetails = new SiteDetails { RegistrationId = registrationId, OrganisationName = "Test Org", SiteAddress = "Test Site address" };
        _reprocessorExporterServiceMock.Setup(s => s.GetSiteDetailsByRegistrationIdAsync(registrationId)).ReturnsAsync(siteDetails);
        _mapperMock.Setup(m => m.Map<SiteDetailsViewModel>(siteDetails)).Returns(new SiteDetailsViewModel { RegistrationId = registrationId,
            LegalDocumentAddress = "LegalDocumentAddress1", Location = "Location1", SiteAddress = "SiteAddress1", SiteGridReference = "SiteGridReference1"
        });

        // Act
        var result = await _controller.UkSiteDetails(registrationId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            viewResult!.ViewData.Keys.Should().Contain(BackLinkViewDataKey);
            viewResult.Model.Should().BeOfType<SiteDetailsViewModel>();
            var viewModel = (SiteDetailsViewModel)viewResult.Model;

            viewModel.RegistrationId.Should().Be(registrationId);
            viewModel.Location.Should().Be("Location1");
            viewModel.LegalDocumentAddress.Should().Be("LegalDocumentAddress1");
            viewModel.SiteAddress.Should().Be("SiteAddress1");
            viewModel.SiteGridReference.Should().Be("SiteGridReference1");
        }
    }

    [TestMethod]
    public async Task UkSiteDetails_WhenTaskIsQueried_ShouldCreateQueryRegistrationSession()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var siteDetails = new SiteDetails
        {
            RegistrationId = registrationId,
            OrganisationName = "Test Org",
            SiteAddress = "Test Site address",
            TaskStatus = RegulatorTaskStatus.Queried,
            RegulatorRegistrationTaskStatusId = Guid.NewGuid()
        };
        _reprocessorExporterServiceMock.Setup(s => s.GetSiteDetailsByRegistrationIdAsync(registrationId)).ReturnsAsync(siteDetails);

        _mapperMock.Setup(m => m.Map<QueryRegistrationSession>(siteDetails)).Returns(new QueryRegistrationSession
        {
            OrganisationName = siteDetails.OrganisationName,
            RegistrationId = siteDetails.RegistrationId,
            RegulatorRegistrationTaskStatusId = siteDetails.RegulatorRegistrationTaskStatusId!.Value,
            PagePath = string.Empty
        });
        
        // Act
        await _controller.UkSiteDetails(registrationId);

        // Assert
        journeySession.ReprocessorExporterSession.QueryMaterialSession.Should().BeNull();
        journeySession.ReprocessorExporterSession.QueryRegistrationSession.Should().NotBeNull();

        var queryRegistrationSession = journeySession.ReprocessorExporterSession.QueryRegistrationSession;
        queryRegistrationSession!.PagePath.Should().Be(PagePath.UkSiteDetails);
        queryRegistrationSession.OrganisationName.Should().Be(siteDetails.OrganisationName);
        queryRegistrationSession.RegulatorRegistrationTaskStatusId.Should().Be(siteDetails.RegulatorRegistrationTaskStatusId.Value);
        queryRegistrationSession.RegistrationId.Should().Be(siteDetails.RegistrationId);
    }

    [TestMethod]
    public async Task CompleteUkSiteDetails_WhenTaskComplete_RedirectToManageRegistrations()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.UkSiteDetails);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteUkSiteDetails(registrationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task WasteCarrierDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");

        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.WasteCarrierDetails(registrationId);
        });
    }

    [TestMethod]
    public async Task WasteCarrierDetails_ReturnView()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.WasteCarrierDetails);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var wasteCarrierDetails = new WasteCarrierDetails { RegistrationId = registrationId, OrganisationName = "Test Org", SiteAddress = "Test Site address" };
        _reprocessorExporterServiceMock.Setup(s => s.GetWasteCarrierDetailsByRegistrationIdAsync(registrationId)).ReturnsAsync(wasteCarrierDetails);

        var expectedViewModel = new WasteCarrierDetailsViewModel
        {
            RegistrationId = registrationId, OrganisationName = "Test Org", SiteAddress = "SiteAddress1"
        };

        _mapperMock.Setup(m => m.Map<WasteCarrierDetailsViewModel>(wasteCarrierDetails)).Returns(expectedViewModel);

        // Act
        var result = await _controller.WasteCarrierDetails(registrationId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            viewResult!.ViewData.Keys.Should().Contain(BackLinkViewDataKey);
            viewResult.Model.Should().BeOfType<WasteCarrierDetailsViewModel>();
            var viewModel = (WasteCarrierDetailsViewModel)viewResult.Model;
            viewModel.Should().Be(expectedViewModel);
        }
    }

    [TestMethod]
    public async Task WasteCarrierDetails_WhenTaskIsQueried_ShouldCreateQueryRegistrationSession()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var wasteCarrierDetails = new WasteCarrierDetails
        {
            RegistrationId = registrationId,
            OrganisationName = "Test Org",
            SiteAddress = "Test Site address",
            TaskStatus = RegulatorTaskStatus.Queried,
            RegulatorRegistrationTaskStatusId = Guid.NewGuid()
        };
        _reprocessorExporterServiceMock.Setup(s => s.GetWasteCarrierDetailsByRegistrationIdAsync(registrationId)).ReturnsAsync(wasteCarrierDetails);

        _mapperMock.Setup(m => m.Map<QueryRegistrationSession>(wasteCarrierDetails)).Returns(new QueryRegistrationSession
        {
            OrganisationName = wasteCarrierDetails.OrganisationName,
            RegistrationId = wasteCarrierDetails.RegistrationId,
            RegulatorRegistrationTaskStatusId = wasteCarrierDetails.RegulatorRegistrationTaskStatusId!.Value,
            PagePath = string.Empty
        });

        // Act
        await _controller.WasteCarrierDetails(registrationId);

        // Assert
        journeySession.ReprocessorExporterSession.QueryMaterialSession.Should().BeNull();
        journeySession.ReprocessorExporterSession.QueryRegistrationSession.Should().NotBeNull();

        var queryRegistrationSession = journeySession.ReprocessorExporterSession.QueryRegistrationSession;
        queryRegistrationSession!.PagePath.Should().Be(PagePath.WasteCarrierDetails);
        queryRegistrationSession.OrganisationName.Should().Be(wasteCarrierDetails.OrganisationName);
        queryRegistrationSession.RegulatorRegistrationTaskStatusId.Should().Be(wasteCarrierDetails.RegulatorRegistrationTaskStatusId.Value);
        queryRegistrationSession.RegistrationId.Should().Be(wasteCarrierDetails.RegistrationId);
    }

    [TestMethod]
    public async Task CompleteWasteCarrierDetails_WhenTaskComplete_RedirectToManageRegistrations()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.WasteCarrierDetails);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteWasteCarrierDetails(registrationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task AuthorisedMaterials_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.AuthorisedMaterials(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task AuthorisedMaterials_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null); // Simulating missing Referer header

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetAuthorisedMaterialsByRegistrationIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationAuthorisedMaterials
            {
                OrganisationName = "TestOrg", SiteAddress = "Site address"
            });

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.AuthorisedMaterials(registrationMaterialId);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task CompleteQueryMaterialTask_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.CompleteQueryMaterialTask);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorApplicationTaskStatusAsync(It.IsAny<UpdateMaterialTaskStatusRequest>()))
            .Returns(Task.CompletedTask);
        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });

        // Act
        var result = await _controller.QueryMaterialTask(new QueryMaterialTaskViewModel { Comments = "", TaskName = RegulatorTaskType.MaterialDetailsAndContact, RegistrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC") });

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new QueryMaterialTaskViewModel();

        _controller.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _controller.QueryMaterialTask(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("QueryMaterialTask.cshtml");
        }
    }

    [TestMethod]
    public async Task CompleteRegistrationMaterialTask_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.CompleteQueryMaterialTask);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorApplicationTaskStatusAsync(It.IsAny<UpdateMaterialTaskStatusRequest>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.QueryRegistrationTask(new QueryRegistrationTaskViewModel { Comments = "", TaskName = RegulatorTaskType.MaterialDetailsAndContact, RegistrationId = registrationId });

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task CompleteAuthorisedMaterials_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.AuthorisedMaterials);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteAuthorisedMaterials(registrationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task InputsAndOutputs_WhenRequestValid_ShouldReturnView()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.InputsAndOutputs);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        _reprocessorExporterServiceMock.Setup(x => x.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialReprocessingIO
            {
                OrganisationName = "Test Organisation",
                SiteAddress = "Test Site address",
                MaterialName = "Plastic",
                SourcesOfPackagingWaste = "N/A",
                PlantEquipmentUsed = "N/A",
                ReprocessingPackagingWasteLastYearFlag = true,
                UKPackagingWasteTonne = 100,
                NonUKPackagingWasteTonne = 50,
                NotPackingWasteTonne = 10,
                SenttoOtherSiteTonne = 5,
                ContaminantsTonne = 2,
                ProcessLossTonne = 1,
                TotalOutputs = 95,
                TotalInputs = 100
            });

        // Act
        var result = await _controller.InputsAndOutputs(registrationMaterialId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var modelResult = viewResult.Model as RegistrationMaterialReprocessingIOViewModel;

        using (new AssertionScope())
        {
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<RegistrationMaterialReprocessingIOViewModel>();            
            modelResult.Should().NotBeNull();
            modelResult.RegistrationMaterialId.Should().Be(registrationMaterialId);
        }
    }

    [TestMethod]
    public async Task InputsAndOutputs_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange: Mock _sessionManager to return null
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.InputsAndOutputs(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task InputsAndOutputs_ShouldReturnCorrectViewModel()
    {
        // Arrange
        Guid registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var registrationMaterialReprocessing = new RegistrationMaterialReprocessingIO
        {
            OrganisationName = "Test Organisation",
            SiteAddress = "Test Site address",
            MaterialName = "Plastic",
            SourcesOfPackagingWaste = "Test",
            PlantEquipmentUsed = "Test"
        };

        var expectedViewModel = new RegistrationMaterialReprocessingIOViewModel
        {
            RegistrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            RegistrationMaterialReprocessingIO = registrationMaterialReprocessing
        };

        _reprocessorExporterServiceMock.Setup(m => m.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(registrationMaterialReprocessing);

        // Act
        var result = await _controller.InputsAndOutputs(registrationMaterialId);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<RegistrationMaterialReprocessingIOViewModel>();

            var model = viewResult.Model as RegistrationMaterialReprocessingIOViewModel;
            model.Should().BeEquivalentTo(expectedViewModel);
        }
    }

    [TestMethod]
    public async Task CompleteInputsAndOutputs_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");

        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteInputsAndOutputs(registrationMaterialId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(
                new RegistrationMaterialSamplingPlan { OrganisationName = "Test Org", MaterialName = "Plastic" });

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(
                new RegistrationMaterialSamplingPlan { OrganisationName = "Test Org", MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(registrationMaterialId);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRequestValid_ShouldReturnView()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.SamplingInspection);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        _reprocessorExporterServiceMock.Setup(x => x.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialSamplingPlan
            {
                OrganisationName = "Test org",
                MaterialName = "Plastic",
                Files = new List<RegistrationMaterialSamplingPlanFile>
                {
                    new RegistrationMaterialSamplingPlanFile
                    {
                        Filename = $"FileName.pdf",
                        FileUploadType = "",
                        FileUploadStatus = "",
                        DateUploaded = DateTime.UtcNow,
                        UpdatedBy = "Test User",
                        Comments = "Test comment",
                        FileId = Guid.NewGuid().ToString()
                    }
                }
            });

        // Act
        var result = await _controller.SamplingInspection(registrationMaterialId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var modelResult = viewResult.Model as RegistrationMaterialSamplingInspectionViewModel;

        using (new AssertionScope())
        {
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<RegistrationMaterialSamplingInspectionViewModel>();
            modelResult.Should().NotBeNull();
            modelResult.RegistrationMaterialId.Should().Be(registrationMaterialId);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.SamplingInspection(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(
                new RegistrationMaterialSamplingPlan { OrganisationName = "Test Org", MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(registrationMaterialId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(
                new RegistrationMaterialSamplingPlan { OrganisationName = "Test Org", MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(registrationMaterialId);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteSamplingInspection_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);
        
        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteSamplingInspection(registrationMaterialId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task QueryRegistrationTask_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryRegistrationTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.SiteAddressAndContactDetails);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryRegistrationTask_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryRegistrationTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.SiteAddressAndContactDetails);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }
    
    [TestMethod]
    public async Task QueryRegistrationTask_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.QueryRegistrationTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.SiteAddressAndContactDetails);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task QueryRegistrationTask_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.QueryRegistrationTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.SiteAddressAndContactDetails);
        });
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.OverseasReprocessorInterim(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.OverseasReprocessorInterim(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.OverseasReprocessorInterim(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.OverseasReprocessorInterim(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteOverseasReprocessorInterim_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteOverseasReprocessorInterim(registrationMaterialId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryMaterialTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.WasteLicensesPermitsAndExemptions);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryMaterialTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.MaterialsAuthorisedOnSite);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.QueryMaterialTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.MaterialsAuthorisedOnSite);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.QueryMaterialTask(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), RegulatorTaskType.MaterialsAuthorisedOnSite);
        });
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.WasteLicences(_registrationIdUrlValue);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.WasteLicences(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));
        });
    }

    [TestMethod]
    public async Task WasteLicences_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.WasteLicences(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task WasteLicences_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.WasteLicences(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }
    
    [TestMethod]
    public async Task CompleteWasteLicences_WhenTaskComplete_RedirectToManageRegistrations()
    {
        // Arrange
        Guid registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.WasteLicences);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteWasteLicences(registrationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(registrationId);
        }
    }

    [TestMethod]
    public async Task MaterialDetails_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.MaterialDetails(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task MaterialDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () => await _controller.MaterialDetails(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")));
    }

    [TestMethod]
    public async Task CompleteMaterialDetails_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);
        
        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteMaterialDetails(registrationMaterialId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () => await _controller.MaterialWasteLicences(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")));
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var wasteLicences = CreateMaterialWasteLicencesModel();

        _reprocessorExporterServiceMock.Setup(m => m.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(wasteLicences);

        // Act
        var result = await _controller.MaterialWasteLicences(registrationMaterialId);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, _manageRegistrationUrl);
        }
    }

    [TestMethod]
    public async Task MaterialWasteLicences_ShouldReturnCorrectViewModel()
    {
        // Arrange
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var wasteLicences = CreateMaterialWasteLicencesModel();

        _reprocessorExporterServiceMock.Setup(m => m.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(wasteLicences);

        var expectedViewModel = new MaterialWasteLicencesViewModel
        {
            ReferenceNumberLabel = "Test",
            CapacityPeriod = "Annually",
            CapacityTonne = 50000,
            LicenceNumbers = ["DFG34573453, ABC34573453, GHI34573453"],
            MaterialName = "Plastic",
            MaximumReprocessingCapacityTonne = 10000,
            MaximumReprocessingPeriod = "Per Month",
            PermitType = "Waste Exemption",
        };

        _mapperMock.Setup(m => m.Map<MaterialWasteLicencesViewModel>(wasteLicences))
            .Returns(expectedViewModel);

        // Act
        var result = await _controller.MaterialWasteLicences(registrationMaterialId);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<MaterialWasteLicencesViewModel>();

            var model = viewResult.Model as MaterialWasteLicencesViewModel;
            model.Should().BeSameAs(expectedViewModel);
        }
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenTaskIsQueried_ShouldCreateQueryMaterialSession()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        JourneySession journeySession = new JourneySession();

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var wasteLicences = CreateMaterialWasteLicencesModel(RegulatorTaskStatus.Queried);

        _reprocessorExporterServiceMock.Setup(m => m.GetWasteLicenceByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(wasteLicences);

        _mapperMock.Setup(m => m.Map<QueryMaterialSession>(wasteLicences)).Returns(new QueryMaterialSession
        {
            OrganisationName = wasteLicences.OrganisationName,
            RegistrationMaterialId = wasteLicences.RegistrationMaterialId,
            RegulatorApplicationTaskStatusId = wasteLicences.RegulatorApplicationTaskStatusId!.Value,
            PagePath = string.Empty
        });

        // Act
        await _controller.MaterialWasteLicences(registrationMaterialId);

        // Assert
        journeySession.ReprocessorExporterSession.QueryRegistrationSession.Should().BeNull();
        journeySession.ReprocessorExporterSession.QueryMaterialSession.Should().NotBeNull();

        var queryMaterialSession = journeySession.ReprocessorExporterSession.QueryMaterialSession;
        queryMaterialSession!.PagePath.Should().Be(PagePath.MaterialWasteLicences);
        queryMaterialSession.OrganisationName.Should().Be(wasteLicences.OrganisationName);
        queryMaterialSession.RegulatorApplicationTaskStatusId.Should().Be(wasteLicences.RegulatorApplicationTaskStatusId.Value);
        queryMaterialSession.RegistrationMaterialId.Should().Be(wasteLicences.RegistrationMaterialId);
    }

    [TestMethod]
    public async Task CompleteMaterialWasteLicences_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _reprocessorExporterServiceMock.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = _registrationIdUrlValue, MaterialName = "Plastic" });
        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteMaterialWasteLicences(registrationMaterialId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageRegistrations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task DownloadSamplingFile_WhenFileExists_ReturnsFileContentResult()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var fileId = Guid.NewGuid();
        const string filename = "test-file.txt";
        var fileBytes = Encoding.UTF8.GetBytes("Fake file content");
        const string contentType = "application/octet-stream";

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileBytes)
        };
        httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = filename
        };

        _reprocessorExporterServiceMock
            .Setup(service => service.DownloadSamplingInspectionFile(It.Is<FileDownloadRequest>(r =>
                r.FileId == fileId && r.FileName == filename)))
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _controller.DownloadSamplingAndInspectionFile(registrationMaterialId, filename, fileId);

        // Assert
        result.Should().BeOfType<FileContentResult>();

        var fileResult = result as FileContentResult;
        using (new AssertionScope())
        {
            fileResult!.ContentType.Should().Be(contentType);
            fileResult.FileDownloadName.Should().Be(filename);
            fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
        }

        httpResponseMessage.Dispose();
    }

    [TestMethod]
    public async Task DownloadSamplingFile_WhenFileNotFound_ReturnsNotFound()
    {
        // Arrange
        const int registrationMaterialId = 5678;
        const string filename = "missing-file.pdf";
        var fileId = Guid.NewGuid();

        var notFoundResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _reprocessorExporterServiceMock
            .Setup(service => service.DownloadSamplingInspectionFile(It.IsAny<FileDownloadRequest>()))
            .ReturnsAsync(notFoundResponse);

        notFoundResponse.Dispose();

        // Act
        var result = await _controller.DownloadSamplingAndInspectionFile(registrationMaterialId, filename, fileId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [TestMethod]
    public async Task DownloadSamplingFile_UsesReprocessorExporterServiceAndReturnsFileContent()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var fileId = Guid.NewGuid();
        const string filename = "sample.pdf";
        var fileBytes = Encoding.UTF8.GetBytes("Fake file content");
        const string contentType = "application/octet-stream";

        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileBytes)
        };
        httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        {
            FileName = filename
        };

        _reprocessorExporterServiceMock
            .Setup(service => service.DownloadSamplingInspectionFile(It.Is<FileDownloadRequest>(r =>
                r.FileId == fileId && r.FileName == filename)))
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _controller.DownloadSamplingAndInspectionFile(registrationMaterialId, filename, fileId);

        // Assert
        result.Should().BeOfType<FileContentResult>();

        var fileResult = result as FileContentResult;
        using (new AssertionScope())
        {
            fileResult!.ContentType.Should().Be(contentType);
            fileResult.FileDownloadName.Should().Be(filename);
            fileResult.FileContents.Should().BeEquivalentTo(fileBytes);
        }

        // Also assert that the correct service method was called
        _reprocessorExporterServiceMock.Verify(service =>
            service.DownloadSamplingInspectionFile(It.Is<FileDownloadRequest>(r =>
                r.FileId == fileId && r.FileName == filename)), Times.Once);

        httpResponseMessage.Dispose();
    }

    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }

    private static RegistrationMaterialWasteLicence CreateMaterialWasteLicencesModel(RegulatorTaskStatus taskStatus = RegulatorTaskStatus.NotStarted)
    {
        var wasteLicences = new RegistrationMaterialWasteLicence
        {
            OrganisationName = "TestOrg",
            PermitType = "PermitType",
            LicenceNumbers = [],
            MaximumReprocessingPeriod = "Max period",
            MaterialName = "Plastic",
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            RegistrationId = Guid.NewGuid(),
            RegistrationMaterialId = Guid.NewGuid(),
            TaskStatus = taskStatus

        };
        return wasteLicences;
    }
}