namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection(SequentialCollection.Sequential)]
public class InviteApprovedPersonTests : IntegrationTestBase
{
    private const string PathBase = "/regulators";

    protected override IDictionary<string, string?>? AdditionalConfiguration => new Dictionary<string, string?>
    {
        ["IntegrationTests:HonourSignInCookie"] = "true"
    };

    [Fact]
    public async Task Submit_WhenFacadeReturnsConflict_ShowsEmailAlreadyExistsError()
    {
        // Arrange
        SetupUserAccountsMock();
        var organisationId = Guid.NewGuid();
        const string invitedEmail = "invitee@test.example";
        const string addRemoveApprovedUsersPath = "/api/organisations/add-remove-approved-users";

        FacadeServer.Given(Request.Create().UsingPost().WithPath(addRemoveApprovedUsersPath))
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithHeader("Content-Type", "application/problem+json")
                .WithBody(JsonSerializer.Serialize(new { status = 409, title = "USER_ALREADY_INVITED" })));

        // Act - walk the invite journey so the session is populated as it would be for a real user
        await GetOk($"{PathBase}/confirm-remove-user" +
                    $"?userName=Jane%20Doe&organisationName=Acme%20Ltd&connExternalId={Guid.NewGuid()}&organisationId={organisationId}");
        await GetOk(InviteUrl("invite-new-approved-person-name"));
        await PostOk(InviteUrl("invite-new-approved-person-name"), ("FirstName", "Ina"), ("LastName", "Vitee"));
        await PostOk(InviteUrl("invite-new-approved-person-email"), ("Email", invitedEmail));
        var response = await PostOk(InviteUrl("invite-new-approved-person-submit"));

        // Assert - user is sent back to the email page with the error
        var page = EnterPersonEmailPageModel.FromContent(await response.Content.ReadAsStringAsync());
        page.ErrorSummaryMessages.Should().ContainSingle().Which.Should().Be("Email address already exists");

        // Assert - exactly one call to the facade (no retry), carrying what the user entered
        var calls = FacadeServer.LogEntries
            .Where(e => e.RequestMessage.Path == addRemoveApprovedUsersPath)
            .ToList();
        calls.Should().ContainSingle();

        using var body = JsonDocument.Parse(calls[0].RequestMessage.Body!);
        body.RootElement.GetProperty("invitedPersonEmail").GetString().Should().Be(invitedEmail);
        body.RootElement.GetProperty("organisationId").GetGuid().Should().Be(organisationId);
    }

    private static string InviteUrl(string page) => $"{PathBase}/invite-new-approved-person/{page}";

    private async Task GetOk(string url)
    {
        using var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> PostOk(string url, params (string Key, string Value)[] fields)
    {
        using var content = new FormUrlEncodedContent(fields.ToDictionary(f => f.Key, f => f.Value));
        var response = await Client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
