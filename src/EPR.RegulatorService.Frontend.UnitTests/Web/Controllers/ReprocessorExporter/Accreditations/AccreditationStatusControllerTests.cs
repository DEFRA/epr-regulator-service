using System.Diagnostics.CodeAnalysis;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Extensions;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using FluentValidation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Accreditations;

[TestClass]
public class AccreditationStatusControllerTests
{
    private const string BackLinkViewDataKey = "BackLinkToDisplay";
    private const int DeterminationWeeks = 12;

    private AccreditationStatusController _controller;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<IMapper> _mapperMock;
    private Mock<HttpContext> _httpContextMock = null!;
    private Mock<IReprocessorExporterService> _mockReprocessorExporterService;


    private Mock<IValidator<IdAndYearRequest>> _validatorMock;
    private Mock<IOptions<ReprocessorExporterConfig>> _configOptionsMock;
    private AccreditationStatusSession _accreditationStatusSession;
    private JourneySession _journeySession;

    private readonly Guid _accreditationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
    private readonly Guid _registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4773");
    private readonly int _year = 2025;

    [TestInitialize]
    public void Setup()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _configurationMock = new Mock<IConfiguration>();
        _mapperMock = new Mock<IMapper>();
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
            .Returns("/accreditations");

        _configurationMock
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _accreditationStatusSession = CreateJourneySession(_registrationId, _accreditationId)
            .ReprocessorExporterSession.AccreditationStatusSession;

        _journeySession = new JourneySession
        {
            ReprocessorExporterSession = { AccreditationStatusSession = _accreditationStatusSession }
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_journeySession);

        _validatorMock = new Mock<IValidator<IdAndYearRequest>>();
        _mockReprocessorExporterService = new Mock<IReprocessorExporterService>();
        _configOptionsMock = new Mock<IOptions<ReprocessorExporterConfig>>();

        var reprocessorConfigMock = new Mock<IOptions<ReprocessorExporterConfig>>();
        reprocessorConfigMock.Setup(c => c.Value).Returns(new ReprocessorExporterConfig { DeterminationWeeks = DeterminationWeeks });


        _controller = new AccreditationStatusController(
            _mapperMock.Object,
            _validatorMock.Object,
            _mockReprocessorExporterService.Object,
            _sessionManagerMock.Object,
            _configurationMock.Object,
            reprocessorConfigMock.Object
        );

        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _controller.FeesDue(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"), 2025));
    }

    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndAccreditationStatusSessionIsNull_ShouldCreateNewSessionFromRegistrationMaterialDetail()
    {
        // Arrange
        var paymentFees = CreateAccreditationPaymentFees(_accreditationId);

        _journeySession.ReprocessorExporterSession.AccreditationStatusSession = null;

        _mockReprocessorExporterService.Setup(r => r.GetPaymentFeesByAccreditationIdAsync(paymentFees.AccreditationId))
            .ReturnsAsync(paymentFees);

        _mapperMock.Setup(m => m.Map<AccreditationStatusSession>(paymentFees))
            .Returns(new AccreditationStatusSession
            {
                MaterialName = "Test Material",
                OrganisationName = "Test Organisation",
                AccreditationId = paymentFees.AccreditationId,
                RegistrationId = _registrationId,
                Year = 2025,
                ApplicationReferenceNumber = paymentFees.ApplicationReferenceNumber,
            });

        // Act
        await _controller.FeesDue(paymentFees.AccreditationId, _year);

        // Assert
        _journeySession.ReprocessorExporterSession.AccreditationStatusSession.Should().NotBeNull();
    }

    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new FeesDueViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _mapperMock.Setup(m =>
                m.Map<FeesDueViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _controller.FeesDue(_accreditationId, _year);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("FeesDue.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndSessionIsNull_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new PaymentCheckViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _journeySession.ReprocessorExporterSession.AccreditationStatusSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _controller.PaymentCheck());
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndValidModelState_ShouldUpdateSession()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            FullPaymentMade = true
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        await _controller.PaymentCheck(viewModel);

        // Assert
        _accreditationStatusSession.FullPaymentMade.Should().Be(viewModel.FullPaymentMade);
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndFullPaymentIsMade_ShouldRedirectToPaymentMethod()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = true };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _controller.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentMethod");
            redirectResult.ControllerName.Should().Be("AccreditationStatus");
        }
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndFullPaymentIsNotMade_ShouldRedirectToQueryTask()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = false };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _controller.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("QueryAccreditationTask");
            redirectResult.ControllerName.Should().Be("AccreditationStatus");
        }
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor };

        _controller.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _controller.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("PaymentCheck.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentMethod_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new PaymentMethodViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _controller.PaymentMethod();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("PaymentMethod.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentMethod_WhenCalledWithViewModelAndValidModelState_ShouldUpdateSession()
    {
        // Arrange
        var viewModel = new PaymentMethodViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            PaymentMethod = PaymentMethodType.AllTypes.First()
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        await _controller.PaymentMethod(viewModel);

        // Assert
        _accreditationStatusSession.PaymentMethod.Should().Be(viewModel.PaymentMethod);
    }

    [TestMethod]
    public async Task PaymentMethod_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToPaymentDate()
    {
        // Arrange
        var viewModel = new PaymentMethodViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            PaymentMethod = PaymentMethodType.AllTypes.First()
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _controller.PaymentMethod(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentDate");
            redirectResult.ControllerName.Should().Be("AccreditationStatus");
        }
    }

    [TestMethod]
    public async Task PaymentMethod_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new PaymentMethodViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _controller.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _controller.PaymentMethod(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("PaymentMethod.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentDate_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new PaymentDateViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _controller.PaymentDate();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("PaymentDate.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentDate_WhenCalledWithViewModelAndValidModelState_ShouldUpdateSession()
    {
        // Arrange
        var viewModel = new PaymentDateViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            Day = 1,
            Month = 2,
            Year = _year
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        await _controller.PaymentDate(viewModel);

        // Assert
        _accreditationStatusSession.PaymentDate.Should().Be(new DateTime(viewModel.Year.Value, viewModel.Month.Value, viewModel.Day.Value));
    }

    [TestMethod]
    public async Task PaymentDate_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToPaymentReview()
    {
        // Arrange
        var viewModel = new PaymentDateViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            Day = 1,
            Month = 2,
            Year = _year
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _controller.PaymentDate(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentReview");
            redirectResult.ControllerName.Should().Be("AccreditationStatus");
        }
    }

    [TestMethod]
    public async Task PaymentDate_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new PaymentDateViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _controller.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _controller.PaymentDate(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("PaymentDate.cshtml");
        }
    }

    [TestMethod]
    public async Task PaymentReview_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new PaymentReviewViewModel
        {
            MaterialName = "Plastic"
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentReviewViewModel>(_journeySession.ReprocessorExporterSession.AccreditationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _controller.PaymentReview();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("PaymentReview.cshtml");
        }
    }

    [TestMethod]
    public async Task MarkAccreditationAsDulyMadeAsync_WhenCalledWithValidSession_ShouldCallFacade()
    {
        // Arrange
        var accreditationId = _accreditationStatusSession.AccreditationId;

        // Act
        await _controller.MarkAsDulyMade();

        // Assert
        _mockReprocessorExporterService.Verify(
            x => x.MarkAccreditationAsDulyMadeAsync(
                accreditationId,
                It.IsAny<AccreditationMarkAsDulyMadeRequest>()),
            Times.Once);
    }

    [TestMethod]
    public async Task MarkAccreditationAsDulyMadeAsync_WhenCalledWithValidSession_ShouldRedirectToManageRegistrations()
    {
        // Act
        var response = await _controller.MarkAsDulyMade();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("ManageAccreditations");
        }
    }

    [TestMethod]
    public async Task AccreditationBusinessPlan_Should_ReturnResult()
    {
        // Arrange
        var journeySession = CreateJourneySession(_registrationId, _accreditationId);
        journeySession.RegulatorSession.Journey.Add(PagePath.AccreditationBusinessPlan);

        var queryNotes = new List<QueryNote>();
        

        var queryNote1 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "First Note"
        };

        var queryNote2 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        var queryNote3 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        queryNotes.Add(queryNote1);
        queryNotes.Add(queryNote2);
        queryNotes.Add(queryNote3);

        var viewModel = new AccreditationBusinessPlanViewModel
        {
            AccreditationId = _accreditationId,
            BusinessCollectionsNotes = string.Empty,
            BusinessCollectionsPercentage = 0.00M,
            CommunicationsNotes = string.Empty,
            CommunicationsPercentage = 0.20M,
            InfrastructureNotes = "Infrastructure notes testing",
            InfrastructurePercentage = 0.30M,
            MaterialName = "Plastic",
            NewMarketsNotes = "New Market Testing notes",
            NewMarketsPercentage = 0.40M,
            NewUsersRecycledPackagingWasteNotes = string.Empty,
            NewUsersRecycledPackagingWastePercentage = 0.25M,
            NotCoveredOtherCategoriesNotes = string.Empty,
            NotCoveredOtherCategoriesPercentage = 5.00M,
            OrganisationName = "",
            RecycledWasteNotes = "No recycled waste notes at this time",
            RecycledWastePercentage = 10.00M,
            SiteAddress = "To Be Confirmed",
            TaskStatus = "Reviewed",
            QueryNotes = queryNotes           
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
       
        _mockReprocessorExporterService.Setup(x => x.GetAccreditionBusinessPlanByIdAsync(_accreditationId))
            .Returns(It.IsAny<Task<AccreditationBusinessPlanDto>>);

        // Act
        var result = await _controller.CompleteAccreditationBusinessPlan(_accreditationId);

        // Assert
        Assert.IsNotNull(result);       
    }

    [TestMethod]
    public async Task AccreditationBusinessPlan_Post_Should_Be_RedirectToActionResult()
    {
        // Arrange
        var journeySession = CreateJourneySession(_registrationId, _accreditationId);
        journeySession.RegulatorSession.Journey.Add(PagePath.AccreditationBusinessPlan);

        var queryNotes = new List<QueryNote>();


        var queryNote1 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "First Note"
        };

        var queryNote2 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        var queryNote3 = new QueryNote
        {
            CreatedBy = _accreditationId,
            CreatedDate = DateTime.Now,
            Notes = "Second Note"
        };

        queryNotes.Add(queryNote1);
        queryNotes.Add(queryNote2);
        queryNotes.Add(queryNote3);       

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);
        var dto = new AccreditationBusinessPlanDto {
            AccreditationId = _accreditationId,
            BusinessCollectionsNotes = string.Empty,
            BusinessCollectionsPercentage = 0.00M,
            CommunicationsNotes = string.Empty,
            CommunicationsPercentage = 0.20M,
            InfrastructureNotes = "Infrastructure notes testing",
            InfrastructurePercentage = 0.30M,
            MaterialName = "Plastic",
            NewMarketsNotes = "New Market Testing notes",
            NewMarketsPercentage = 0.40M,
            NewUsersRecycledPackagingWasteNotes = string.Empty,
            NewUsersRecycledPackagingWastePercentage = 0.25M,
            NotCoveredOtherCategoriesNotes = string.Empty,
            NotCoveredOtherCategoriesPercentage = 5.00M,
            OrganisationName = "",
            RecycledWasteNotes = "No recycled waste notes at this time",
            RecycledWastePercentage = 10.00M,
            SiteAddress = "To Be Confirmed",
            TaskStatus = "Reviewed",
            QueryNotes = null
        };

        _mockReprocessorExporterService.Setup(x => x.GetAccreditionBusinessPlanByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.CompleteAccreditationBusinessPlan(_accreditationId);

        var redirectResult = result as RedirectToActionResult;

        // Assert
        redirectResult.Should().BeOfType<RedirectToActionResult>();       
    }


    [TestMethod]
    public async Task AccreditationBusinessPlan_Should_ReturnView()
    {
        // Arrange
        var journeySession = CreateJourneySession(_registrationId, _accreditationId);
        journeySession.RegulatorSession.Journey.Add(PagePath.AccreditationBusinessPlan);

        int year = _year;

        InitialiseAccreditationStatusSessionIfNotExists(journeySession, _accreditationId, year);

        // Act
        var result = await _controller.AccreditationBusinessPlan(_accreditationId, year);

        var viewResult = result as ViewResult;

        // Assert
        viewResult.Should().BeOfType<ViewResult>();
    }

    private static JourneySession CreateJourneySession(Guid registrationId, Guid accreditationId) =>
    new JourneySession
    {
        ReprocessorExporterSession = new ReprocessorExporterSession
        {
            RegistrationId = registrationId,
            AccreditationStatusSession = new AccreditationStatusSession
            {
                OrganisationName = "Test Organisation",
                MaterialName = "Test Material",
                RegistrationId = registrationId,
                AccreditationId = accreditationId,
                Year = 2025
            }
        }
    };

    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }

    private static AccreditationMaterialPaymentFees CreateAccreditationPaymentFees(Guid accreditationId) =>
        new()
        {
            OrganisationName = "Test Organisation",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            AccreditationId = accreditationId,
            MaterialName = "Plastic",
            ApplicationReferenceNumber = "123456789",
            Regulator = "Regulator"
        };


    private static void InitialiseAccreditationStatusSessionIfNotExists(JourneySession session, Guid accreditationId, int year)
    {
        if (session.ReprocessorExporterSession.AccreditationStatusSession != null &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.AccreditationId == accreditationId &&
            session.ReprocessorExporterSession.AccreditationStatusSession!.Year == year)
        {
            return;
        }

        var accreditationStatusSession = new AccreditationStatusSession
        {
            AccreditationId = accreditationId,
            Year = year,
            OrganisationName = null!,
            MaterialName = null!
        };

        session.ReprocessorExporterSession.AccreditationStatusSession = accreditationStatusSession;
    }
}
