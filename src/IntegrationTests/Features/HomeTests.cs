namespace IntegrationTests.Features;

using AwesomeAssertions;
using AwesomeAssertions.Execution;
using IntegrationTests.Builders;
using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;

[Collection(SequentialCollection.Sequential)]
public class HomeTests : IntegrationTestBase
{
    [Theory]
    [InlineData(null, "Environment Agency", 1, "Jane", "Smith")]
    [InlineData(null, "Natural Resources Wales", 4, "John", "Jones")]
    [InlineData("cy", "Natural Resources Wales", 4, "Sioned", "Evans")] // note org name is NOT translated currently
    public async Task HomePage_ShowsDynamicOrganisationAndPersonName(
        string? culture,
        string organisationName,
        int nationId,
        string firstName,
        string lastName)
    {
        // Arrange
        SetupUserAccountsMock(UserAccountBuilder.Default()
            .WithOrganisationName(organisationName)
            .WithNationId(nationId)
            .WithFirstName(firstName)
            .WithLastName(lastName));
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<HomePageModel>(
            requestUri: "/regulators/home");

        // Assert - organisation and person name come from account data, not localisation
        using (new AssertionScope())
        {
            page.OrganisationName.Should().Be(organisationName);
            page.PersonName.Should().Be($"{firstName} {lastName}");
        }
    }
}
