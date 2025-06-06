using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations;

[TestClass]
public class QueryControllerTests : RegistrationControllerTestBase
{
    private QueryController _queryController; // System under test

    [TestInitialize]
    public void TestInitialize()
    {
        CreateCommonMocks();
        CreateSessionMock();
        
        _journeySession.ReprocessorExporterSession.QueryMaterialSession = new QueryMaterialSession
        {
            OrganisationName = "TestOrg",
            RegistrationMaterialId = Guid.NewGuid(),
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            PagePath = PagePath.FeesDue
        };

        var configurationMock = CreateConfigurationMock();
        
        _queryController = new QueryController(
            _mapperMock.Object,
            _reprocessorExporterServiceMock.Object,
            _sessionManagerMock.Object,
            configurationMock.Object);
        
        _queryController.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _queryController.AddMaterialQueryNote());
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenQueryMaterialSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryMaterialSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _queryController.AddMaterialQueryNote());
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new AddQueryNoteViewModel();

        _journeySession.ReprocessorExporterSession.QueryMaterialSession = CreateQueryMaterialSession();

        _mapperMock.Setup(m =>
                m.Map<AddQueryNoteViewModel>(_journeySession.ReprocessorExporterSession.QueryMaterialSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _queryController.AddMaterialQueryNote();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("AddQueryNote.cshtml");
        }
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenCalledWithViewModelAndValidModelState_ShouldCallApi()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryMaterialSession = CreateQueryMaterialSession();

        var expectedTaskId = _journeySession.ReprocessorExporterSession.QueryMaterialSession!.RegulatorApplicationTaskStatusId;

        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };
        
        // Act
        await _queryController.AddMaterialQueryNote(viewModel);

        // Assert
        expectedTaskId.Should().NotBeEmpty();
        _reprocessorExporterServiceMock.Verify(m => m.AddMaterialQueryNoteAsync(
                expectedTaskId,
                It.Is<AddNoteRequest>(r => r.Note == viewModel.Note)),
                Times.Once);
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToReturnPage()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryMaterialSession = CreateQueryMaterialSession();

        string expectedReturnPath = _journeySession.ReprocessorExporterSession.QueryMaterialSession!.PagePath;

        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };

        // Act
        var response = await _queryController.AddMaterialQueryNote(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            expectedReturnPath.Should().NotBeNullOrEmpty();
            redirectResult.ActionName.Should().Be(expectedReturnPath);
            redirectResult.ControllerName.Should().Be(PagePath.ReprocessorExporterRegistrations);
        }
    }

    [TestMethod]
    public async Task AddMaterialQueryNote_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };

        _journeySession.ReprocessorExporterSession.QueryMaterialSession = CreateQueryMaterialSession();

        _queryController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _queryController.AddMaterialQueryNote(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("AddQueryNote.cshtml");
        }
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _queryController.AddRegistrationQueryNote());
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenQueryRegistrationSessionIsNull_ShouldThrowException()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryRegistrationSession = null;

        // Act/Assert
        await Assert.ThrowsExceptionAsync<SessionException>(() => _queryController.AddRegistrationQueryNote());
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenCalledWithValidSession_ShouldReturnViewResult()
    {
        // Arrange
        var expectedViewModel = new AddQueryNoteViewModel();

        _journeySession.ReprocessorExporterSession.QueryRegistrationSession = CreateQueryRegistrationSession();

        _mapperMock.Setup(m =>
                m.Map<AddQueryNoteViewModel>(_journeySession.ReprocessorExporterSession.QueryRegistrationSession))
            .Returns(expectedViewModel);

        // Act
        var response = await _queryController.AddRegistrationQueryNote();

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewName.Should().EndWith("AddQueryNote.cshtml");
        }
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenCalledWithViewModelAndValidModelState_ShouldCallApi()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryRegistrationSession = CreateQueryRegistrationSession();

        var expectedTaskId = _journeySession.ReprocessorExporterSession.QueryRegistrationSession!.RegulatorRegistrationTaskStatusId;

        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };

        // Act
        await _queryController.AddRegistrationQueryNote(viewModel);

        // Assert
        expectedTaskId.Should().NotBeEmpty();
        _reprocessorExporterServiceMock.Verify(m => m.AddRegistrationQueryNoteAsync(
                expectedTaskId,
                It.Is<AddNoteRequest>(r => r.Note == viewModel.Note)),
                Times.Once);
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenCalledWithViewModelAndValidModelState_ShouldRedirectToReturnPage()
    {
        // Arrange
        _journeySession.ReprocessorExporterSession.QueryRegistrationSession = CreateQueryRegistrationSession();

        string expectedReturnPath = _journeySession.ReprocessorExporterSession.QueryRegistrationSession!.PagePath;

        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };

        // Act
        var response = await _queryController.AddRegistrationQueryNote(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<RedirectToActionResult>();

            var redirectResult = (RedirectToActionResult)response;
            expectedReturnPath.Should().NotBeNullOrEmpty();
            redirectResult.ActionName.Should().Be(expectedReturnPath);
            redirectResult.ControllerName.Should().Be(PagePath.ReprocessorExporterRegistrations);
        }
    }

    [TestMethod]
    public async Task AddRegistrationQueryNote_WhenCalledWithViewModelAndInvalidModelState_ShouldRedisplayView()
    {
        // Arrange
        var viewModel = new AddQueryNoteViewModel
        {
            Note = "Test note",
            OrganisationName = "TestOrg"
        };

        _journeySession.ReprocessorExporterSession.QueryRegistrationSession = CreateQueryRegistrationSession();

        _queryController.ModelState.AddModelError("Test", "Error");

        // Act
        var response = await _queryController.AddRegistrationQueryNote(viewModel);

        // Assert
        using (new AssertionScope())
        {
            response.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)response;
            viewResult.ViewName.Should().EndWith("AddQueryNote.cshtml");
        }
    }

    private QueryMaterialSession CreateQueryMaterialSession() =>
        new()
        {
            OrganisationName = "TestOrg",
            RegistrationMaterialId = Guid.NewGuid(),
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            PagePath = PagePath.FeesDue
        };

    private QueryRegistrationSession CreateQueryRegistrationSession() =>
        new()
        {
            OrganisationName = "TestOrg",
            RegistrationId = Guid.NewGuid(),
            RegulatorRegistrationTaskStatusId = Guid.NewGuid(),
            PagePath = PagePath.FeesDue
        };
}
