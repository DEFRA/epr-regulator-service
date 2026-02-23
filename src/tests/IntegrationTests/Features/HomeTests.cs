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
    [InlineData(null, 1, "Environment Agency", "Environment Agency", "Jane", "Smith")]
    [InlineData("cy", 1, "Environment Agency", "Asiantaeth yr Amgylchedd", "Jane", "Smith")]
    [InlineData(null, 2, "Northern Ireland Environment Agency", "Northern Ireland Environment Agency", "Patrick", "Murphy")]
    [InlineData("cy", 2, "Northern Ireland Environment Agency", "Asiantaeth Amgylchedd Gogledd Iwerddon", "Patrick", "Murphy")]
    [InlineData(null, 3, "Scottish Environment Protection Agency", "Scottish Environment Protection Agency", "Hamish", "MacLeod")]
    [InlineData("cy", 3, "Scottish Environment Protection Agency", "Asiantaeth Diogelu Amgylchedd yr Alban", "Hamish", "MacLeod")]
    [InlineData(null, 4, "Natural Resources Wales", "Natural Resources Wales", "John", "Jones")]
    [InlineData("cy", 4, "Natural Resources Wales", "Cyfoeth Naturiol Cymru", "Sioned", "Evans")]
    public async Task HomePage_ShowsDynamicOrganisationAndPersonName(
        string? culture,
        int nationId,
        string userOrganisationName,
        string expectedOrganisationName,
        string firstName,
        string lastName)
    {
        // Arrange
        SetupUserAccountsMock(UserAccountBuilder.Default()
            .WithOrganisationName(userOrganisationName)
            .WithNationId(nationId)
            .WithFirstName(firstName)
            .WithLastName(lastName));
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<HomePageModel>(
            requestUri: "/regulators/home");

        // Assert
        using (new AssertionScope())
        {
            page.OrganisationName.Should().Be(expectedOrganisationName);
            page.PersonName.Should().Be($"{firstName} {lastName}");
        }
    }
}
