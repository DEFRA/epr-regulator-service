using EPR.RegulatorService.Frontend.Core.Configs;
using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

using FluentAssertions.Execution;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class RegistrationStatusControllerTests : RegistrationControllerTestBase
{
    private const int DeterminationWeeks = 12;
    private RegistrationStatusController _registrationStatusController; // System under test

    private RegistrationStatusSession _registrationStatusSession;

    [TestInitialize]
    public void TestInitialize()
    {
        Guid registrationMaterialId = Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21");

        CreateCommonMocks();
        CreateSessionMock();
        
        var validatorMock = new Mock<IValidator<IdRequest>>();
        var configurationMock = CreateConfigurationMock();

        _registrationStatusSession = CreateRegistrationStatusSession(registrationMaterialId);
        _journeySession.ReprocessorExporterSession.RegistrationStatusSession = _registrationStatusSession;
        
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
        await Assert.ThrowsExceptionAsync<SessionException>(() => _registrationStatusController.FeesDue(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC")));
    }
    
    [TestMethod]
    public async Task FeesDue_WhenCalledWithIdAndRegistrationStatusSessionIsNull_ShouldCreateNewSessionFromRegistrationMaterialDetail()
    {
        // Arrange
        var paymentFees = CreateRegistrationPaymentFees(Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"), DateTime.Now.AddDays(+16), DateTime.Now.AddDays(-5));

        _journeySession.ReprocessorExporterSession.RegistrationStatusSession = null;

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
        var response = await _registrationStatusController.FeesDue(Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"));

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
    public async Task PaymentCheck_WhenCalledWithViewModelAndSessionIsNull_ShouldThrowException()
    {
        // Arrange
        var expectedViewModel = new PaymentCheckViewModel
        {
            ApplicationType = ApplicationOrganisationType.Reprocessor
        };

        _journeySession.ReprocessorExporterSession.RegistrationStatusSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _registrationStatusController.PaymentCheck(expectedViewModel));
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
    public async Task PaymentCheck_WhenCalledWithViewModelAndFullPaymentIsMade_ShouldRedirectToPaymentMethod()
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
    [DataRow(RegulatorTaskStatus.Completed)]
    [DataRow(RegulatorTaskStatus.CannotStartYet)]
    [DataRow(RegulatorTaskStatus.NotStarted)]
    [DataRow(RegulatorTaskStatus.Started)]
    public async Task PaymentCheck_WhenCalledWithViewModelAndFullPaymentIsNotMadeAndTaskIsNotQueried_ShouldRedirectToQueryPage(RegulatorTaskStatus taskStatus)
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = false };

        _journeySession.ReprocessorExporterSession.RegistrationStatusSession = CreateRegistrationStatusSession(Guid.NewGuid(), taskStatus);

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
    public async Task PaymentCheck_WhenCalledWithViewModelAndFullPaymentIsNotMadeAndTaskIsQueried_ShouldRedirectToAddNotePage()
    {
        // Arrange
        var viewModel = new PaymentCheckViewModel { ApplicationType = ApplicationOrganisationType.Reprocessor, FullPaymentMade = false };

        var registrationSession = CreateRegistrationStatusSession(Guid.NewGuid(), RegulatorTaskStatus.Queried);
        _journeySession.ReprocessorExporterSession.RegistrationStatusSession = registrationSession;

        _mapperMock.Setup(m => m.Map<QueryMaterialSession>(registrationSession)).Returns(new QueryMaterialSession
        {
            OrganisationName = "TEST",
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            RegistrationMaterialId = registrationSession.RegistrationMaterialId,
            PagePath = PagePath.FeesDue
        });

        // Act
        var response = await _registrationStatusController.PaymentCheck(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("AddMaterialQueryNote");
            redirectResult.ControllerName.Should().Be("Query");
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
    [DataRow("2025-06-01", "2025-05-29", 0)] //01June - 29May = 3/7 = 0.42
    [DataRow("2025-06-02", "2025-05-29", 1)] //02June - 29May = 4/7 = 0.57
    [DataRow("2025-06-19", "2025-05-29", 3)] //19June - 29May = 21/7 = 3
    [DataRow("2025-06-24", "2025-05-29", 4)] //24June - 29May = 26/7 = 3.71
    [DataRow("2025-06-25", "2025-05-29", 4)] //19June - 29May = 27/7 = 3.85
    public async Task RegistrationApplicationStatus_WhenCalledAfterDulyMade_ShouldReturnApplicationStatusView(string determinationDateString, string dulyMadeDateString, int expectedDeterminationWeeks)
    {
        // Arrange
        var determinationDateTime = DateTime.Parse(determinationDateString);
        var dulyMadeDateTime = DateTime.Parse(dulyMadeDateString);

        var registrationPaymentFees = CreateRegistrationPaymentFees(Guid.Parse("F267151B-07F0-43CE-BB5B-37671609EB21"), determinationDateTime, dulyMadeDateTime);
        var expectedViewModel = new PaymentReviewViewModel
        {
            MaterialName = "Plastic",
            SubmittedDate = DateTime.Now.AddDays(-7),
            PaymentMethod = PaymentMethodType.BankTransfer,
            PaymentDate = DateTime.Now.AddDays(-7),
            DulyMadeDate = dulyMadeDateTime,
            DeterminationDate = determinationDateTime
        };

        _reprocessorExporterServiceMock.Setup(r => r.GetPaymentFeesByRegistrationMaterialIdAsync(registrationPaymentFees.RegistrationMaterialId))
        .ReturnsAsync(registrationPaymentFees);

        _mapperMock.Setup(m =>
                m.Map<PaymentReviewViewModel>(registrationPaymentFees))
            .Returns(expectedViewModel);

        // Act
        var response = await _registrationStatusController.RegistrationApplicationStatus(registrationPaymentFees.RegistrationMaterialId);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("RegistrationApplicationStatus.cshtml");
            var viewModel = (PaymentReviewViewModel)viewResult.Model;
            viewModel.DeterminationWeeks.Should().Be(expectedDeterminationWeeks);
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

    [TestMethod]
    public async Task AddNote_ShouldRedirectToAddMaterialQueryNotePage()
    {
        // Arrange
        _mapperMock
            .Setup(m => m.Map<QueryMaterialSession>(It.IsAny<RegistrationStatusSession>()))
            .Returns(new QueryMaterialSession
            {
                OrganisationName = "Test",
                RegulatorApplicationTaskStatusId = Guid.NewGuid(),
                RegistrationMaterialId = Guid.NewGuid(),
                PagePath = PagePath.FeesDue
            });

        // Act
        var response = await _registrationStatusController.AddNote();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            redirectResult.ActionName.Should().Be("AddMaterialQueryNote");
            redirectResult.ControllerName.Should().Be("Query");
        }
    }

    private static RegistrationStatusSession CreateRegistrationStatusSession(
        Guid registrationMaterialId,
        RegulatorTaskStatus taskStatus = RegulatorTaskStatus.NotStarted) =>
        new()
        {
            OrganisationName = "Test Organisation",
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            MaterialName = "Plastic",
            TaskStatus = taskStatus,
        };

    private static RegistrationMaterialPaymentFees CreateRegistrationPaymentFees(Guid registrationMaterialId, DateTime determinationDate, DateTime dulyMadeDate) =>
        new()
        {
            OrganisationName = "Test Organisation",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            RegistrationMaterialId = registrationMaterialId,
            RegistrationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC"),
            MaterialName = "Plastic",
            ApplicationReferenceNumber = "123456789",
            Regulator = "Regulator",
            SubmittedDate = DateTime.Now.AddDays(-7),
            PaymentMethod = PaymentMethodType.BankTransfer,
            PaymentDate = DateTime.Now.AddDays(-7),
            DulyMadeDate = dulyMadeDate,
            DeterminationDate = determinationDate
        };
}