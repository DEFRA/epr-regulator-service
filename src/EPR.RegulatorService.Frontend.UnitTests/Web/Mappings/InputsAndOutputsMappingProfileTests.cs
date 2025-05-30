using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

using Frontend.Core.Models.ReprocessorExporter.Registrations;

[TestClass]
public class InputsAndOutputsMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<InputsAndOutputsMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithRegistrationMaterialReprocessingIO_ShouldReturnQueryMaterialSession()
    {
        // Arrange
        var inputsAndOutputs = new RegistrationMaterialReprocessingIO
        {
            OrganisationName = "Test Organisation",
            SiteAddress = "Site address",
            RegistrationId = Guid.NewGuid(),
            RegistrationMaterialId = Guid.NewGuid(),
            MaterialName = "Plastic",
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            TaskStatus = RegulatorTaskStatus.Completed,
            SourcesOfPackagingWaste = "Sources of waste",
            PlantEquipmentUsed = "Plant equipment"
        };

        // Act
        var result = _mapper.Map<QueryMaterialSession>(inputsAndOutputs);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be(inputsAndOutputs.OrganisationName);
        result.SiteAddress.Should().Be(inputsAndOutputs.SiteAddress);
        result.RegistrationMaterialId.Should().Be(inputsAndOutputs.RegistrationMaterialId);
        result.RegulatorApplicationTaskStatusId.Should().Be(inputsAndOutputs.RegulatorApplicationTaskStatusId.Value);
    }
}