namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Accreditations;

using System.Net;
using System.Text;

using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

using FluentValidation;

using Frontend.Core.Models.ReprocessorExporter.Accreditations;
using Frontend.Web.Sessions;
using Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

[TestClass]
public class AccreditationsControllerTests
{
    private AccreditationsController _controller;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<HttpContext> _httpContextMock = null!;
    private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;
    private JourneySession _journeySession;
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    private readonly string _manageAccreditationUrl = $"manage-accreditations?id={Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163").ToString()}&year=2025";
    private readonly Guid _accreditationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4163");
    private readonly Guid _registrationId = Guid.Parse("9D16DEF0-D828-4800-83FB-2B60907F4773");
    private readonly int _year = 2025;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpContextMock = new Mock<HttpContext>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _configurationMock = new Mock<IConfiguration>();
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

        _journeySession = CreateJourneySession(_registrationId, _accreditationId);

        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(_journeySession);

        _reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();

        var validatorMock = new Mock<IValidator<IdRequest>>();

        _controller = new AccreditationsController(_sessionManagerMock.Object, _reprocessorExporterServiceMock.Object, _configurationMock.Object, validatorMock.Object);
        _controller.ControllerContext.HttpContext = _httpContextMock.Object;
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionContainsJourney_ShouldSetBackLinkToPreviousPage()
    {
        // Arrange
        _journeySession.RegulatorSession.Journey.Add(_manageAccreditationUrl);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByAccreditationIdAsync(_accreditationId))
            .ReturnsAsync(
                new AccreditationSamplingPlan() { MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(_accreditationId, _year);

        // Assert
        using (new AssertionScope())
        {
            var viewResult = (ViewResult)result;
            AssertBackLink(viewResult, _manageAccreditationUrl);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRequestValid_ShouldReturnView()
    {
        _journeySession.RegulatorSession.Journey.Add(PagePath.SamplingInspection);
        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);

        _reprocessorExporterServiceMock.Setup(x => x.GetSamplingPlanByAccreditationIdAsync(_accreditationId))
            .ReturnsAsync(new AccreditationSamplingPlan
            {
                MaterialName = "Plastic",
                Files = new List<AccreditationSamplingPlanFile>
                {
                    new AccreditationSamplingPlanFile
                    {
                        Filename = $"FileName.pdf",
                        FileUploadType = "",
                        FileUploadStatus = "",
                        DateUploaded = DateTime.UtcNow,
                        UpdatedBy = "Test User",
                        FileId = Guid.NewGuid().ToString()
                    }
                }
            });

        // Act
        var result = await _controller.SamplingInspection(_accreditationId, _year);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        var modelResult = viewResult.Model as AccreditationSamplingInspectionViewModel;

        using (new AssertionScope())
        {
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<AccreditationSamplingInspectionViewModel>();
            modelResult.Should().NotBeNull();
            modelResult.AccreditationId.Should().Be(_accreditationId);
        }
    }

    [TestMethod]
    public async Task SamplingInspection_WhenSessionIsNull_ShouldThrowException()
    {
        _sessionManagerMock
            .Setup(m => m.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null!);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<SessionException>(async () =>
        {
            await _controller.SamplingInspection(_accreditationId, _year);
        });
    }

    [TestMethod]
    public async Task SamplingInspection_WhenRefererHeaderIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var accreditationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        const int year = 2025;
        var mockHeaders = new Mock<IHeaderDictionary>();
        mockHeaders.Setup(h => h["Referer"]).Returns((string?)null);
        mockHeaders.Setup(h => h.Referer).Returns((string?)null);

        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders.Object);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByAccreditationIdAsync(accreditationId))
            .ReturnsAsync(
                new AccreditationSamplingPlan { MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(accreditationId, year);

        // Assert
        result.Should().BeOfType<ViewResult>();
        AssertBackLink(result as ViewResult, "/accreditations/" + PagePath.Home);
    }

    [TestMethod]
    public async Task SamplingInspection_WhenHeadersIsMissing_ShouldSetHomeBackLink()
    {
        // Arrange
        var accreditationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        const int year = 2025;
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Headers).Returns((IHeaderDictionary)null);

        _httpContextMock.Setup(c => c.Request).Returns(mockRequest.Object);

        _reprocessorExporterServiceMock
            .Setup(s => s.GetSamplingPlanByAccreditationIdAsync(accreditationId))
            .ReturnsAsync(
                new AccreditationSamplingPlan { MaterialName = "Plastic" });

        // Act
        var result = await _controller.SamplingInspection(accreditationId, year);

        // Assert
        result.Should().BeOfType<ViewResult>();
        AssertBackLink(result as ViewResult, "/accreditations/" + PagePath.Home);
    }

    [TestMethod]
    public async Task CompleteSamplingInspection_WhenTaskComplete_ShouldRedirectToManageRegistrations()
    {
        // Arrange
        var accreditationId = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");
        const int year = 2025;
        var journeySession = new JourneySession();
        journeySession.RegulatorSession.Journey.Add(_manageAccreditationUrl);

        _reprocessorExporterServiceMock.Setup(x => x.UpdateRegulatorAccreditationTaskStatusAsync(It.IsAny<UpdateAccreditationTaskStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CompleteSamplingInspection(accreditationId);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        var redirectToActionResult = result as RedirectToActionResult;
        redirectToActionResult.Should().NotBeNull();

        using (new AssertionScope())
        {
            redirectToActionResult.ActionName.Should().Be("Index");
            redirectToActionResult.ControllerName.Should().Be("ManageAccreditations");
            redirectToActionResult.RouteValues.Should().ContainKey("id");
            redirectToActionResult.RouteValues["id"].Should().Be(_registrationId);
        }
    }

    [TestMethod]
    public async Task DownloadSamplingFile_WhenFileExists_ReturnsFileContentResult()
    {
        // Arrange
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
        var result = await _controller.DownloadSamplingAndInspectionFile(fileId, filename);

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
        const string filename = "missing-file.pdf";
        var fileId = Guid.NewGuid();

        var notFoundResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _reprocessorExporterServiceMock
            .Setup(service => service.DownloadSamplingInspectionFile(It.IsAny<FileDownloadRequest>()))
            .ReturnsAsync(notFoundResponse);

        notFoundResponse.Dispose();

        // Act
        var result = await _controller.DownloadSamplingAndInspectionFile(fileId, filename);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    private static void AssertBackLink(ViewResult viewResult, string expectedBackLink)
    {
        var hasBackLinkKey = viewResult.ViewData.TryGetValue(BackLinkViewDataKey, out var gotBackLinkObject);
        hasBackLinkKey.Should().BeTrue();
        (gotBackLinkObject as string)?.Should().Be(expectedBackLink);
    }

    private static JourneySession CreateJourneySession(Guid registrationId, Guid accreditationId)
    {
        return new JourneySession
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
    }
}