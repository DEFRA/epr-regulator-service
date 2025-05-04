using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class RegistrationsControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private const int RegistrationIdUrlValue = 1234;

    private readonly string _manageRegistrationUrl = $"manage-registrations?id={RegistrationIdUrlValue}";

    private RegistrationsController _controller;
    private Mock<ISessionManager<JourneySession>> _mockSessionManager;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IMapper> _mockMapper;
    private Mock<HttpContext> _httpContextMock = null!;
    private Mock<IReprocessorExporterService> _mockReprocessorExporterService;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpContextMock = new Mock<HttpContext>();
        _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMapper = new Mock<IMapper>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();

        // Set up the Referer header to return a sample URL (or null for different tests)
        mockHeaders.Setup(h => h["Referer"]).Returns("http://previous-page.com");
        mockHeaders.Setup(h => h.Referer).Returns("http://previous-page.com");

        // Set the mock Request to the HttpContext
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);
        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        _mockConfiguration
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession());

        _mockReprocessorExporterService = new Mock<IReprocessorExporterService>();

        _controller = new RegistrationsController(_mockSessionManager.Object, _mockReprocessorExporterService.Object, _mockConfiguration.Object, _mockMapper.Object);

        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task BusinessAddress_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.BusinessAddress(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task BusinessAddress_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = 1234;
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.BusinessAddress(1);
        });
    }

    [TestMethod]
    public async Task UkSiteDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        const int registrationId = 1234;

        // Arrange
        _mockSessionManager
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
        const int registrationId = 1234;
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.UkSiteDetails);
        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var siteDetails = new SiteDetails { Id = registrationId };
        _mockReprocessorExporterService.Setup(s => s.GetSiteDetailsByRegistrationIdAsync(registrationId)).ReturnsAsync(siteDetails);
        _mockMapper.Setup(m => m.Map<SiteDetailsViewModel>(siteDetails)).Returns(new SiteDetailsViewModel { RegistrationId = registrationId,
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
    public async Task CompleteUkSiteDetails_WhenTaskComplete_RedirectToManageRegistrations()
    {
        // Arrange
        const int registrationId = 1234;
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.UkSiteDetails);
        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
    public async Task AuthorisedMaterials_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.AuthorisedMaterials(1);
        });
    }

    [TestMethod]
    public async Task AuthorisedMaterials_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null); // Simulating missing Referer header

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.AuthorisedMaterials(1);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [TestMethod]
    public async Task CompleteQueryMaterialTask_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.CompleteQueryMaterialTask);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorApplicationTaskStatusAsync(It.IsAny<UpdateMaterialTaskStatusRequest>()))
            .Returns(Task.CompletedTask);
        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(1))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = 1, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });

        // Act
        var result = await _controller.QueryMaterialTask(new QueryMaterialTaskViewModel { Comments = "", TaskName = RegulatorTaskType.MaterialDetailsAndContact, RegistrationMaterialId = 1});

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
    public async Task CompleteRegistrationMaterialTask_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.CompleteQueryMaterialTask);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorApplicationTaskStatusAsync(It.IsAny<UpdateMaterialTaskStatusRequest>()))
            .Returns(Task.CompletedTask);
        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(1))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = 1, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });

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
        var registrationId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.AuthorisedMaterials);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
        int registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.InputsAndOutputs);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        _mockReprocessorExporterService.Setup(x => x.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialReprocessingIO
            {
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
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!); // Simulating a null session

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.InputsAndOutputs(1);
        });
    }

    [TestMethod]
    public async Task InputsAndOutputs_ShouldReturnCorrectViewModel()
    {
        // Arrange
        const int registrationMaterialId = 1;
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        var registrationMaterialReprocessing = new RegistrationMaterialReprocessingIO
        {
            MaterialName = "Plastic", SourcesOfPackagingWaste = "Test", PlantEquipmentUsed = "Test"
        };

        var expectedViewModel = new RegistrationMaterialReprocessingIOViewModel
        {
            RegistrationMaterialId = 1,
            RegistrationMaterialReprocessingIO = registrationMaterialReprocessing
        };

        _mockReprocessorExporterService.Setup(m => m.GetReprocessingIOByRegistrationMaterialIdAsync(registrationMaterialId))
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
        var registrationMaterialId = 1234;

        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
            redirectToActionResult.RouteValues["id"].Should().Be(RegistrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.SamplingInspection(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRequestValid_ShouldReturnView()
    {
        // Arrange
        int registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.SamplingInspection);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        _mockReprocessorExporterService.Setup(x => x.GetSamplingPlanByRegistrationMaterialIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialSamplingPlan
            {
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
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.SamplingInspection(1);
        });
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.SamplingInspection(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.SamplingInspection(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.SamplingInspection(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteSamplingInspection_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);


        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
            redirectToActionResult.RouteValues["id"].Should().Be(RegistrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task QueryRegistrationTask_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryRegistrationTask(1, RegulatorTaskType.SiteAddressAndContactDetails);

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
        var result = await _controller.QueryRegistrationTask(1, RegulatorTaskType.SiteAddressAndContactDetails);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryRegistrationTask_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.QueryRegistrationTask(1, RegulatorTaskType.SiteAddressAndContactDetails);

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

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.QueryRegistrationTask(1, RegulatorTaskType.SiteAddressAndContactDetails);

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
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.QueryRegistrationTask(1, RegulatorTaskType.SiteAddressAndContactDetails);
        });
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.OverseasReprocessorInterim(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.OverseasReprocessorInterim(1);
        });
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.OverseasReprocessorInterim(1);

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
        var result = await _controller.OverseasReprocessorInterim(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task OverseasReprocessorInterim_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.OverseasReprocessorInterim(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteOverseasReprocessorInterim_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
            redirectToActionResult.RouteValues["id"].Should().Be(RegistrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.QueryMaterialTask(1, RegulatorTaskType.WasteLicensesPermitsAndExemptions);

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
        var result = await _controller.QueryMaterialTask(1, RegulatorTaskType.MaterialsAuthorisedOnSite);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.QueryMaterialTask(1, RegulatorTaskType.MaterialsAuthorisedOnSite);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.QueryMaterialTask(1, RegulatorTaskType.MaterialsAuthorisedOnSite);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task QueryMaterialTask_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.QueryMaterialTask(1, RegulatorTaskType.MaterialsAuthorisedOnSite);
        });
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.WasteLicences(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task WasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.WasteLicences(1);
        });
    }

    [TestMethod]
    public async Task WasteLicences_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        // Act
        var result = await _controller.WasteLicences(1);

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
        var result = await _controller.WasteLicences(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task WasteLicences_WhenHttpRequestIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        _httpContextMock.Setup(c => c.Request).Returns((HttpRequest)null);

        // Act
        var result = await _controller.WasteLicences(1);

        // Assert
        result.Should().BeOfType<ViewResult>();

        AssertBackLink(result as ViewResult, "/regulators/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteWasteLicences_WhenTaskComplete_RedirectToManageRegistrations()
    {
        // Arrange
        const int registrationId = 1234;
        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(PagePath.WasteLicences);
        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
        journeySession.RegulatorSession.Journey.Add(PagePath.ManageRegistrations);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.MaterialDetails(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, PagePath.ManageRegistrations);
        }
    }

    [TestMethod]
    public async Task MaterialDetails_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () => await _controller.MaterialDetails(1));
    }

    [TestMethod]
    public async Task CompleteMaterialDetails_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);
        
        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
            redirectToActionResult.RouteValues["id"].Should().Be(RegistrationIdUrlValue);
        }
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _mockSessionManager
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () => await _controller.MaterialWasteLicences(1));
    }

    [TestMethod]
    public async Task MaterialWasteLicences_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        // Act
        var result = await _controller.MaterialWasteLicences(1);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;

            AssertBackLink(viewResult, expectedPreviousPage);
        }
    }

    [TestMethod]
    public async Task MaterialWasteLicences_ShouldReturnCorrectViewModel()
    {
        // Arrange
        const string expectedPreviousPage = $"{PagePath.ManageRegistrations}?id=1345";

        JourneySession journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(expectedPreviousPage);

        _mockSessionManager.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

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

        _mockMapper.Setup(m => m.Map<MaterialWasteLicencesViewModel>(It.IsAny<RegistrationMaterialWasteLicence>()))
            .Returns(expectedViewModel);

        // Act
        var result = await _controller.MaterialWasteLicences(1);

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
    public async Task CompleteMaterialWasteLicences_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var registrationMaterialId = 1234;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageRegistrationUrl);

        _mockReprocessorExporterService.Setup(x => x.GetRegistrationMaterialByIdAsync(registrationMaterialId))
            .ReturnsAsync(new RegistrationMaterialDetail { Id = registrationMaterialId, RegistrationId = RegistrationIdUrlValue, MaterialName = "Plastic" });
        _mockReprocessorExporterService.Setup(x => x.UpdateRegulatorRegistrationTaskStatusAsync(It.IsAny<UpdateRegistrationTaskStatusRequest>()))
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
            redirectToActionResult.RouteValues["id"].Should().Be(RegistrationIdUrlValue);
        }
    }
    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }
}