using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings;

using Frontend.Web.Constants;

[TestClass]
public class QueryNoteMappingProfileTests
{
    private IMapper _mapper;

    [TestInitialize]
    public void TestInitialize()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<QueryNoteMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [TestMethod]
    public void Mapping_Configuration_IsValid() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [TestMethod]
    public void Map_WhenCalledWithRegistrationStatusSession_ShouldReturnQueryMaterialSession()
    {
        // Arrange
        var registrationStatusSession = new RegistrationStatusSession
        {
            OrganisationName = "Test Organisation",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            RegistrationId = Guid.NewGuid(),
            RegistrationMaterialId = Guid.NewGuid(),
            MaterialName = "Plastic",
            Regulator = "EA",
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            TaskStatus = RegulatorTaskStatus.Completed,
        };

        // Act
        var result = _mapper.Map<QueryMaterialSession>(registrationStatusSession);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be(registrationStatusSession.OrganisationName);
        result.ApplicationType.Should().Be(registrationStatusSession.ApplicationType);
        result.RegistrationMaterialId.Should().Be(registrationStatusSession.RegistrationMaterialId);
        result.RegulatorApplicationTaskStatusId.Should().Be(registrationStatusSession.RegulatorApplicationTaskStatusId.Value);
    }

    [TestMethod]
    public void Map_WhenCalledWithQueryMaterialSession_ShouldReturnAddMaterialQueryNoteViewModel()
    {
        // Arrange
        var queryMaterialSession = new QueryMaterialSession
        {
            OrganisationName = "Test Organisation",
            ApplicationType = ApplicationOrganisationType.Reprocessor,
            RegistrationMaterialId = Guid.NewGuid(),
            RegulatorApplicationTaskStatusId = Guid.NewGuid(),
            PagePath = PagePath.FeesDue
        };

        // Act
        var result = _mapper.Map<AddQueryNoteViewModel>(queryMaterialSession);

        // Assert
        result.Should().NotBeNull();
        result.OrganisationName.Should().Be(queryMaterialSession.OrganisationName);
        result.SiteAddress.Should().Be(queryMaterialSession.SiteAddress);
        result.ApplicationType.Should().Be(queryMaterialSession.ApplicationType);
        result.Note.Should().BeNull();
    }
}


