using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Sessions;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Controllers.ReprocessorExporter.Registrations
{
[TestClass]
public class RegistrationControllerTests
{
    private RegistrationsController _controller;
        private Mock<ISessionManager<JourneySession>> _mockSessionManager;
        private Mock<IConfiguration> _mockConfiguration;

    [TestInitialize]
        public void TestInitialize()
        {
            _mockSessionManager = new Mock<ISessionManager<JourneySession>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockSessionManager
                .Setup(m => m.GetSessionAsync(It.IsAny<Microsoft.AspNetCore.Http.ISession>()))
                .ReturnsAsync(new JourneySession());

            _controller = new RegistrationsController(_mockSessionManager.Object, _mockConfiguration.Object);
        }

        [TestMethod]
        public async Task UkSiteDetails_WithCorrectModel_ShouldReturnView()
        {
            // Act
            var result = await _controller.UkSiteDetails();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

            var model = viewResult.Model as ManageRegistrationsViewModel;
            model.Should().NotBeNull();
            model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
        }

        [TestMethod]
        public async Task BusinessAddress_WithCorrectModel_ShouldReturnView()
        {
            // Act
            var result = await _controller.BusinessAddress();

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

            var model = viewResult.Model as ManageRegistrationsViewModel;
            model.Should().NotBeNull();
            model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Exporter);
        }

        [TestMethod]
        public async Task AuthorisedMaterials_WithCorrectModel_ShouldReturnView()
    {
        // Act
            var result = await _controller.AuthorisedMaterials();

        // Assert
        result.Should().BeOfType<ViewResult>(); 

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<ManageRegistrationsViewModel>();

            var model = viewResult.Model as ManageRegistrationsViewModel;
            model.Should().NotBeNull();
            model!.ApplicationOrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
        }
    }
}
