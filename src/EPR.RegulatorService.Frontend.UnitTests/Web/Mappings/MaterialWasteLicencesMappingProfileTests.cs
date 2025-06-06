using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Constants.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentAssertions.Execution;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

[TestClass]
public class MaterialWasteLicencesMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MaterialWasteLicencesMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Map_WhenCalledWithRegistrationMaterialWasteLicence_ShouldReturnViewModel()
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence();

        // Act
        var viewModel = _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        using (new AssertionScope())
        {
            viewModel.CapacityTonne.Should().Be(materialWasteLicence.CapacityTonne);
            viewModel.LicenceNumbers.Should().BeEquivalentTo(materialWasteLicence.LicenceNumbers);
            viewModel.MaterialName.Should().Be(materialWasteLicence.MaterialName);
            viewModel.MaximumReprocessingCapacityTonne.Should().Be(materialWasteLicence.MaximumReprocessingCapacityTonne);
            viewModel.PermitType.Should().Be(materialWasteLicence.PermitType);
            viewModel.TaskStatus.Should().Be(materialWasteLicence.TaskStatus);
        }
    }

    [TestMethod]
    [DataRow(PeriodTypes.PerYear, "Annually")]
    [DataRow(PeriodTypes.PerMonth, "Monthly")]
    [DataRow(PeriodTypes.PerWeek, "Weekly")]
    public void Map_WhenPeriodTypeIsValid_ShouldSetPeriodTypeText(string periodType, string expectedText)
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(maximumReprocessingPeriod: periodType, capacityPeriod:periodType);

        // Act
        var viewModel = _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        using (new AssertionScope())
        {
            viewModel.CapacityPeriod.Should().Be(expectedText);
            viewModel.MaximumReprocessingPeriod.Should().Be(expectedText);
        }
    }

    [TestMethod]
    public void Map_WhenCapacityPeriodIsNull_ShouldSetCapacityPeriodToEmptyString()
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(capacityPeriod: null);

        // Act
        var viewModel = _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        using (new AssertionScope())
        {
            viewModel.CapacityPeriod.Should().Be(string.Empty);
        }
    }

    [TestMethod]
    public void Map_WhenCapacityPeriodIsInvalid_ShouldThrowException()
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(capacityPeriod: "UNKNOWN");

        // Act
        Action act = () => _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        act.Should().Throw<AutoMapperMappingException>();
    }

    [TestMethod]
    public void Map_WhenMaximumReprocessingPeriodIsInvalid_ShouldThrowException()
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(maximumReprocessingPeriod: "UNKNOWN");

        // Act
        Action act = () => _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        act.Should().Throw<AutoMapperMappingException>();
    }

    [TestMethod]
    [DataRow(PermitTypes.EnvironmentalPermitOrWasteManagementLicence, "Environment permit or waste management number")]
    [DataRow(PermitTypes.InstallationPermit, "Installation permit number")]
    [DataRow(PermitTypes.PollutionPreventionAndControlPermit, "PPC permit number")]
    [DataRow(PermitTypes.WasteExemption, "Exemption reference(s)")]
    [DataRow(PermitTypes.WasteManagementLicence, "Waste management number")]
    public void Map_WhenPermitTypeIsValid_ShouldSetPeriodTypeText(string permitType, string expectedReferenceNumberLabel)
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(permitType: permitType);

        // Act
        var viewModel = _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        viewModel.ReferenceNumberLabel.Should().Be(expectedReferenceNumberLabel);
    }

    [TestMethod]
    public void Map_WhenPermitTypeIsInvalid_ShouldThrowException()
    {
        // Arrange
        var materialWasteLicence = CreateRegistrationMaterialWasteLicence(permitType: "UNKNOWN");

        // Act
        Action act = () => _mapper.Map<MaterialWasteLicencesViewModel>(materialWasteLicence);

        // Assert
        act.Should().Throw<AutoMapperMappingException>();
    }

    private static RegistrationMaterialWasteLicence CreateRegistrationMaterialWasteLicence(
        string? capacityPeriod = "Per Year",
        string maximumReprocessingPeriod = "Per Year",
        string permitType = "Waste Exemption")
    {
        var materialWasteLicence = new RegistrationMaterialWasteLicence
        {
            OrganisationName = "Test Org",
            CapacityPeriod = capacityPeriod,
            CapacityTonne = 50000,
            LicenceNumbers = ["DFG34573453, ABC34573453, GHI34573453"],
            MaterialName = "Plastic",
            MaximumReprocessingCapacityTonne = 10000,
            MaximumReprocessingPeriod = maximumReprocessingPeriod,
            PermitType = permitType
        };
        return materialWasteLicence;
    }
}