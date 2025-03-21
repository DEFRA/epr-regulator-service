using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services;

[TestClass]
public class MockedRegistrationServiceTests
{
    private MockedRegistrationService _service;

    [TestInitialize]
    public void TestInitialize()
    {
        _service = new MockedRegistrationService();
    }

    [TestMethod]
    public void GetRegistrationById_EvenId_ShouldReturnReprocessor()
    {
        // Arrange
        var id = 2;

        // Act
        var result = _service.GetRegistrationById(id);

        // Assert
        result.Should().NotBeNull();

        using (new AssertionScope())
        {
            result!.Id.Should().Be(id);
            result.OrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
            result.OrganisationName.Should().Be("Green Ltd");
            result.SiteAddress.Should().Be("23 Ruby St, London, E12 3SE");
            result.Regulator.Should().Be("Environment Agency (EA)");
        }
    }

    [TestMethod]
    public void GetRegistrationById_OddId_ShouldReturnExporter()
    {
        // Arrange
        var id = 3;

        // Act
        var result = _service.GetRegistrationById(id);

        // Assert
        result.Should().NotBeNull();

        using (new AssertionScope())
        {
            result!.Id.Should().Be(id);
            result.OrganisationType.Should().Be(ApplicationOrganisationType.Exporter);
            result.OrganisationName.Should().Be("Blue Exports Ltd");
            result.SiteAddress.Should().Be("N/A"); // Exporters have no site address
            result.Regulator.Should().Be("Environment Agency (EA)");
        }
    }

    [TestMethod]
    public void GetRegistrationById_NegativeId_ShouldReturnCorrectType()
    {
        // Arrange
        var id = -4; // Even negative number, should still be Reprocessor

        // Act
        var result = _service.GetRegistrationById(id);

        // Assert
        result.Should().NotBeNull();
        result!.OrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
    }

    [TestMethod]
    public void GetRegistrationById_NonExistentId_ShouldStillReturnValidObject()
    {
        // Arrange
        var id = 9999; // High value, but service doesn't check for real existence

        // Act
        var result = _service.GetRegistrationById(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id); // Service should still return a mock object
    }

    [TestMethod]
    public void GetRegistrationById_WhenIdIs99999_ShouldThrowException()
    {
        // Arrange
        var id = 99999;

        // Act
        var exception = Assert.ThrowsException<Exception>(() => _service.GetRegistrationById(id));

        // Assert
        exception.Message.Should().Be("Mocked exception for testing purposes.");
    }

}
