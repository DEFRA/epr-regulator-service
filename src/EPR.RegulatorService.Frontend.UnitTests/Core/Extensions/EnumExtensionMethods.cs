using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Extensions;

[TestClass]
public class EnumExtensionMethods
{
    [TestMethod]
    public async Task GetDescription_EnumWithDescription_ConvertsSuccessfully()
    {
        string expectedDescription = "OPTION 111";

        string actualDescription = TestEnum.Option1.GetDescription();

        actualDescription.Should().Be(expectedDescription);
    }

    [TestMethod]
    public async Task GetDescription_EnumWithoutDescription_DisplaysOriginalEnum()
    {
        string expectedDescription = "Option2";

        string actualDescription = TestEnum.Option2.GetDescription();

        actualDescription.Should().Be(expectedDescription);
    }

    [TestMethod]
    [DataRow(RegistrationSubmissionOrganisationType.compliance, RegistrationSubmissionType.ComplianceScheme)]
    [DataRow(RegistrationSubmissionOrganisationType.large, RegistrationSubmissionType.Producer)]
    [DataRow(RegistrationSubmissionOrganisationType.small, RegistrationSubmissionType.Producer)]
    [DataRow(RegistrationSubmissionOrganisationType.none, RegistrationSubmissionType.NotSet)]
    public void Should_Get_Correct_RegistrationSubmissionType(
        RegistrationSubmissionOrganisationType registrationSubmissionOrganisationType,
        RegistrationSubmissionType expectedRegistrationSubmissionType)
    {
        // Arrange

        // Act
        var actualRegistrationSubmissionType = registrationSubmissionOrganisationType.GetRegistrationSubmissionType();

        // Assert
        actualRegistrationSubmissionType.Should().Be(expectedRegistrationSubmissionType);
    }

    [TestMethod]
    public async Task GetDescription_EnumWithNull_ReturnEmptyString()
    {
        // Arrange
        string expectedDescription = string.Empty;
        Enum genericNullEnum = null;

        // Act
        string actualDescription = genericNullEnum.GetDescription();

        // Assert
        actualDescription.Should().Be(expectedDescription);
    }
}
