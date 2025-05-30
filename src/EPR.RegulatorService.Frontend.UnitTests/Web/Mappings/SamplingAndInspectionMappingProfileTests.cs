using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

using Frontend.Core.Models.ReprocessorExporter.Registrations;

[TestClass]
public class SamplingAndInspectionMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SamplingAndInspectionMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithRegistrationMaterialSamplingPlan_ShouldReturnQueryMaterialSession()
    {
        // Arrange
        var registrationStatusSession = new RegistrationMaterialSamplingPlan
        {
            OrganisationName = "Test Organisation",
            SiteAddress = "Site address",
            RegistrationId = Guid.NewGuid(),
            RegistrationMaterialId = Guid.NewGuid(),
            MaterialName = "Plastic",
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            TaskStatus = RegulatorTaskStatus.Completed,
        };

        // Act
        var result = _mapper.Map<QueryMaterialSession>(registrationStatusSession);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        result.SiteAddress.Should().Be(registrationStatusSession.SiteAddress);
        result.RegistrationMaterialId.Should().Be(registrationStatusSession.RegistrationMaterialId);
        result.RegulatorApplicationTaskStatusId.Should().Be(registrationStatusSession.RegulatorApplicationTaskStatusId.Value);
    }
}