using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.Validations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using FluentAssertions;
using FluentAssertions.Execution;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Accreditations;

[TestClass]
public class ManageAccreditationsControllerTests
{
    private ManageAccreditationsController _controller;
    private Mock<IReprocessorExporterService> _serviceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IValidator<IdAndYearRequest>> _validatorMock;
    private Mock<ISessionManager<JourneySession>> _sessionManagerMock;
    private Mock<IConfiguration> _configurationMock;
    private Mock<HttpContext> _httpContextMock;
    private Mock<ISession> _sessionMock;

    private static readonly Guid ValidRegistrationGuid = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private const int ValidYear = 2025;
    private const string BackLinkViewDataKey = "BackLinkToDisplay";

    [TestInitialize]
    public void SetUp()
    {
        _serviceMock = new Mock<IReprocessorExporterService>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<IdAndYearRequest>>();
        _sessionManagerMock = new Mock<ISessionManager<JourneySession>>();
        _configurationMock = new Mock<IConfiguration>();
        var configSectionMock = new Mock<IConfigurationSection>();

        configSectionMock.Setup(s => s.Value).Returns("/regulators");
        _configurationMock.Setup(c => c.GetSection(ConfigKeys.PathBase)).Returns(configSectionMock.Object);

        _sessionManagerMock
            .Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(new JourneySession());

        _httpContextMock = new Mock<HttpContext>();
        _sessionMock = new Mock<ISession>();

        _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);

        _controller = new ManageAccreditationsController(
            _serviceMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _sessionManagerMock.Object,
            _configurationMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            }
        };
    }

    [TestMethod]
    public async Task Index_WithValidRequest_ShouldReturnCorrectViewModel()
    {
        // Arrange
        var request = new IdAndYearRequest { Id = ValidRegistrationGuid, Year = ValidYear };
        var registration = new Registration
        {
            IdGuid = ValidRegistrationGuid,
            OrganisationName = "Mock Org",
            Regulator = "EA",
            SiteAddress = "123 Fake Street",
            OrganisationType = ApplicationOrganisationType.Reprocessor,
            Materials = [],
            Tasks = []
        };

        var expectedViewModel = new ManageAccreditationsViewModel { Id = ValidRegistrationGuid };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _serviceMock.Setup(s => s.GetRegistrationByIdWithAccreditationsAsync(ValidRegistrationGuid, ValidYear))
            .ReturnsAsync(registration);

        _mapperMock.Setup(m => m.Map<ManageAccreditationsViewModel>(registration))
            .Returns(expectedViewModel);

        // Act
        var result = await _controller.Index(ValidRegistrationGuid, ValidYear);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<ManageAccreditationsViewModel>();
            viewResult.Model.Should().Be(expectedViewModel);
            viewResult.ViewData.Should().ContainKey(BackLinkViewDataKey);
        }
    }

    [TestMethod]
    public async Task Index_WithInvalidId_ShouldThrowValidationException()
    {
        // Arrange
        var invalidId = Guid.Empty;
        var invalidYear = 1999;

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure(nameof(IdAndYearRequest.Id), "Invalid ID"),
            new ValidationFailure(nameof(IdAndYearRequest.Year), "Invalid year")
        };

        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(v => v.Validate(It.IsAny<IdAndYearRequest>()))
            .Returns(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<ValidationException>(async () =>
        {
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _controller.Index(invalidId, invalidYear);
        });

        exception.Errors.Should().NotBeNullOrEmpty();
        exception.Errors.Select(e => e.ErrorMessage).Should().Contain("Invalid ID");
        exception.Errors.Select(e => e.ErrorMessage).Should().Contain("Invalid year");
    }

    [TestMethod]
    public async Task Index_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var request = new IdAndYearRequest { Id = ValidRegistrationGuid, Year = ValidYear };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _serviceMock.Setup(s => s.GetRegistrationByIdWithAccreditationsAsync(ValidRegistrationGuid, ValidYear))
            .ThrowsAsync(new Exception("Something went wrong"));

        // Act
        Func<Task> act = async () => await _controller.Index(ValidRegistrationGuid, ValidYear);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Something went wrong");
    }
}