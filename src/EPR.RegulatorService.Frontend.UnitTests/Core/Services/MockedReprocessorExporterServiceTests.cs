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
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(
            Guid.Parse("839544fd-9b08-4823-9277-5615072a6803"), 2025);

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
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(
            Guid.Parse("11dec0d4-b0db-44a6-84f3-3de06e46262c"), 2025);

        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Steel");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Status.Should().Be(ApplicationStatus.Withdrawn);
            material.Accreditations.Should().ContainSingle(a => a.Status == "Granted");
        }
    }

    [TestMethod]
    public async Task ShouldThrow_WhenNoAccreditationsExistForYear()
    {
        Func<Task> act = () => _service.GetRegistrationByIdWithAccreditationsAsync(
            Guid.Parse("23456789-bbbb-cccc-dddd-222222222222"), 2025); // only has 2024

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No accreditations found for any materials in year 2025.");
    }

    [TestMethod]
    public async Task ShouldThrow_WhenMoreThanOneAccreditationExistsForSameYear()
    {
        Func<Task> act = () => _service.GetRegistrationByIdWithAccreditationsAsync(
            Guid.Parse("34567890-cccc-dddd-eeee-333333333333"), 2025);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("More than one accreditation found for MaterialId * in year 2025.");
    }

    [TestMethod]
    public async Task ShouldDisplayMaterial_WhenMultipleYearsExist_Filter2026()
    {
        var result = await _service.GetRegistrationByIdWithAccreditationsAsync(
            Guid.Parse("45678901-dddd-eeee-ffff-444444444444"), 2026);

        var material = result.Materials.FirstOrDefault(m => m.MaterialName == "Paper");

        using (new AssertionScope())
        {
            material.Should().NotBeNull();
            material!.Accreditations.Should().ContainSingle(a => a.AccreditationYear == 2026);
        }
    }

}
