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
    public async Task GetRegistrationById_ValidId_ShouldReturnReprocessor()
    {
        // Arrange
        var id = Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50");

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

        // Assert
        result.Should().NotBeNull();

        using (new AssertionScope())
        {
            result!.Id.Should().Be(id);
            result.OrganisationType.Should().Be(ApplicationOrganisationType.Reprocessor);
            result.OrganisationName.Should().Be("MOCK Green Ltd");
            result.SiteAddress.Should().Be("23 Ruby St, London, E12 3SE");
            result.Regulator.Should().Be("Environment Agency (EA)");
        }
    }

    [TestMethod]
    public async Task GetRegistrationById_ValidId_ShouldReturnExporter()
    {
        // Arrange
        var id = Guid.Parse("3B0AE13B-4162-41E6-8132-97B4D6865DAC");

        // Act
        var result = await _service.GetRegistrationByIdAsync(id);

        // Assert
        result.Should().NotBeNull();

        using (new AssertionScope())
        {
            result!.Id.Should().Be(id);
            result.OrganisationType.Should().Be(ApplicationOrganisationType.Exporter);
            result.OrganisationName.Should().Be("MOCK Blue Exports Ltd");
            result.SiteAddress.Should().Be("N/A"); // Exporters have no site address
            result.Regulator.Should().Be("Environment Agency (EA)");
        }
    }

    [TestMethod]
    public async Task GetRegistrationById_WhenIdIsEmpty_ShouldThrowException()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var exception = await Assert.ThrowsExceptionAsync<NotFoundException>(() => _service.GetRegistrationByIdAsync(id));

        // Assert
        exception.Message.Should().Be("Mocked exception for testing purposes.");
    }

    [TestMethod]
    public async Task ShouldDisplayMaterial_WhenMaterialStatusIsGranted_ShouldBeDisplayed()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.NewGuid()); // ID doesn't matter in mock
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Plastic");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Granted);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Granted");
        }
    }


    [TestMethod]
    public async Task ShouldNotDisplayMaterial_WhenMaterialStatusIsWithdrawn_EvenIfAccreditationIsGranted()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Steel");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Withdrawn);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Granted");
        }
    }

    [TestMethod]
    public async Task ShouldNotDisplayMaterial_WhenStatusIsNull_AndNoAccreditations()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Glass");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().BeNull();
            material.Accreditations.Should().BeEmpty();
        }
    }

    [TestMethod]
    public async Task ShouldNotDisplayMaterial_WhenStatusIsStarted_AndNoAccreditations()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Wood");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Started);
            material.Accreditations.Should().BeEmpty();
        }
    }

    [TestMethod]
    public async Task ShouldNotDisplayMaterial_WhenStatusIsGranted_ButAccreditationIsStarted()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Cardboard");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Granted);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Started");
        }
    }

    [TestMethod]
    public async Task ShouldNotDisplayMaterial_WhenStatusIsGranted_ButAccreditationIsWithdrawn()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Rubber");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Granted);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Withdrawn");
        }
    }

    [TestMethod]
    public async Task ShouldDisplayMaterial_WhenStatusIsGranted_AndAccreditationIsGranted()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(Guid.Parse("84FFEFDC-2306-4854-9B93-4A8A376D7E50"));
        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Aluminium");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Granted);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Granted");
        }
    }
}
