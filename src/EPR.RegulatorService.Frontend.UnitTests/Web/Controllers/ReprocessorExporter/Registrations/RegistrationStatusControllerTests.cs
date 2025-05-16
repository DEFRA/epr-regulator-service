using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;
using EPR.RegulatorService.Frontend.Web.Sessions;

using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class RegistrationStatusControllerTests
{
    private const int DeterminationWeeks = 12;
    private RegistrationStatusController _registrationStatusController; // System under test

    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;

    private JourneySession _journeySession;
    private RegistrationStatusSession _registrationStatusSession;

    [TestInitialize]
    public void TestInitialize()
    {
        const int registrationMaterialId = 123;

        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _mapperMock = new Mock<IMapper>();
        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();

        var validatorMock = new Mock<IValidator<IdRequest>>();
        var configurationSectionMock = new Mock<IConfigurationSection>();
        var configurationMock = new Mock<IConfiguration>();

        configurationSectionMock
            .Setup(section => section.Value)
            .Returns("/regulators");

        configurationMock
            .Setup(config => config.GetSection(ConfigKeys.PathBase))
            .Returns(configurationSectionMock.Object);

        _registrationStatusSession = CreateRegistrationStatusSession(registrationMaterialId);
        _journeySession = new JourneySession
        {
            ReprocessorExporterSession = { RegistrationStatusSession = _registrationStatusSession }
        };

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_journeySession);

        var reprocessorConfigMock = new Mock<IOptions<ReprocessorExporterConfig>>();
        reprocessorConfigMock.Setup(c => c.Value).Returns(new ReprocessorExporterConfig{ DeterminationWeeks = DeterminationWeeks});

        _registrationStatusController = new RegistrationStatusController(
                _mapperMock.Object,
                validatorMock.Object,
                _reprocessorExporterServiceMock.Object,
                _sessionManagerMock.Object,
                configurationMock.Object,
                reprocessorConfigMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        _registrationStatusController.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _registrationStatusController.FeesDue(1));
    }

    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndApplicationUpdateSessionIsNull_ShouldCreateNewSessionFromRegistrationMaterialDetail()
    {
        // Arrange
        var paymentFees = CreateRegistrationPaymentFees(123);

        _journeySession.ReprocessorExporterSession.ApplicationUpdateSession = null;

        _reprocessorExporterServiceMock.Setup(r => r.GetPaymentFeesByRegistrationMaterialIdAsync(paymentFees.RegistrationMaterialId))
            .ReturnsAsync(paymentFees);

        _mapperMock.Setup(m => m.Map<RegistrationStatusSession>(paymentFees))
            .Returns(CreateRegistrationStatusSession(paymentFees.RegistrationMaterialId));

        // Act
        await _registrationStatusController.FeesDue(paymentFees.RegistrationMaterialId);

        // Assert
        _journeySession.ReprocessorExporterSession.RegistrationStatusSession.Should().NotBeNull();
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
                m.Map<FeesDueViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.FeesDue(1);

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
    public async Task PaymentCheck_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new PaymentCheckViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.PaymentCheck();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("PaymentCheck.cshtml");
        }
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
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        await _registrationStatusController.PaymentCheck(viewModel);

        // Assert
        _registrationStatusSession.FullPaymentMade.Should().Be(viewModel.FullPaymentMade);
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToPaymentMethod()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = true };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _registrationStatusController.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentMethod");
            redirectResult.ControllerName.Should().Be("RegistrationStatus");
        }
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToQueryTask()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = false };

        _mapperMock.Setup(m =>
                m.Map<PaymentCheckViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _registrationStatusController.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("QueryMaterialTask");
            redirectResult.ControllerName.Should().Be("Registrations");
        }
    }

    [TestMethod]
    public async Task PaymentCheck_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor };

        _registrationStatusController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _registrationStatusController.PaymentCheck(viewModel);

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
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.PaymentMethod();

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
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        await _registrationStatusController.PaymentMethod(viewModel);

        // Assert
        _registrationStatusSession.PaymentMethod.Should().Be(viewModel.PaymentMethod);
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
                m.Map<PaymentMethodViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _registrationStatusController.PaymentMethod(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentDate");
            redirectResult.ControllerName.Should().Be("RegistrationStatus");
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

        _registrationStatusController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _registrationStatusController.PaymentMethod(viewModel);

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
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.PaymentDate();

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
            Year = 2023
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        await _registrationStatusController.PaymentDate(viewModel);

        // Assert
        _registrationStatusSession.PaymentDate.Should().Be(new DateTime(viewModel.Year.Value, viewModel.Month.Value, viewModel.Day.Value));
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
            Year = 2023
        };

        _mapperMock.Setup(m =>
                m.Map<PaymentDateViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(viewModel);

        // Act
        var response = await _registrationStatusController.PaymentDate(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("PaymentReview");
            redirectResult.ControllerName.Should().Be("RegistrationStatus");
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

        _registrationStatusController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _registrationStatusController.PaymentDate(viewModel);

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
                m.Map<PaymentReviewViewModel>(_journeySession.ReprocessorExporterSession.RegistrationStatusSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.PaymentReview();

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
    public async Task MarkAsDulyMade_WhenCalledWithValidSession_ShouldCallFacade()
    {
        // Arrange
        var registrationMaterialId = _registrationStatusSession.RegistrationMaterialId;

        // Act
        await _registrationStatusController.MarkAsDulyMade();

        // Assert
        _reprocessorExporterServiceMock.Verify(
            x => x.MarkAsDulyMadeAsync(
                registrationMaterialId,
                It.IsAny<MarkAsDulyMadeRequest>()),
            Times.Once);

        _reprocessorExporterServiceMock.Verify(
            x => x.SubmitOfflinePaymentAsync(
                It.IsAny<OfflinePaymentRequest>()),
            Times.Once);
    }

    [TestMethod]
    public async Task MarkAsDulyMade_WhenCalledWithValidSession_ShouldRedirectToManageRegistrations()
    {
        // Act
        var response = await _registrationStatusController.MarkAsDulyMade();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("ManageRegistrations");
        }
    }

    private static RegistrationStatusSession CreateRegistrationStatusSession(int registrationMaterialId) =>
        new()
        {
            OrganisationName = "Test Organisation",
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = 1,
            MaterialName = "Plastic"
        };
    private static RegistrationMaterialPaymentFees CreateRegistrationPaymentFees(int registrationMaterialId) =>
        new()
        {
            OrganisationName = "Test Organisation",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = 1,
            MaterialName = "Plastic",
            ApplicationReferenceNumber = "123456789",
            Regulator = "Regulator"
        };
}