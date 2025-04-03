namespace EPR.RegulatorService.Frontend.UnitTests.Core.Services;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Exceptions;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

using FluentAssertions.Execution;

[TestClass]
public class MockedReprocessorExporterServiceTests
{
    private MockedReprocessorExporterService _service;

    [TestInitialize]
    public void TestInitialize() => _service = new MockedReprocessorExporterService();

    [TestMethod]
    public async Task GetRegistrationById_EvenId_ShouldReturnReprocessor()
    {
        // Arrange
        var id = 2;

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

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
    public async Task GetRegistrationById_OddId_ShouldReturnExporter()
    {
        // Arrange
        var id = 3;

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

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
    public async Task GetRegistrationById_NegativeId_ShouldReturnCorrectType()
    {
        // Arrange
        var id = -4; // Even negative number, should still be Reprocessor

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.OrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
    }

    [TestMethod]
    public async Task GetRegistrationById_NonExistentId_ShouldStillReturnValidObject()
    {
        // Arrange
        var id = 9999; // High value, but service doesn't check for real existence

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id); // Service should still return a mock object
    }

    [TestMethod]
    public async Task GetRegistrationById_WhenIdIs99999_ShouldThrowException()
    {
        // Arrange
        var id = 99999;

        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotFoundException>( () => _service.GetRegistrationByIdAsync(id));

        // Assert
        exception.Message.Should().Be("Mocked exception for testing purposes.");
    }

}
