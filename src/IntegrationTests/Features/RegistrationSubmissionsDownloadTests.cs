namespace IntegrationTests.Features;

using System.Net;
using System.Text.Json;
using AwesomeAssertions;

using Builders;

using Infrastructure;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection(SequentialCollection.Sequential)]
public class RegistrationSubmissionsDownloadTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegistrationSubmissions_FullDownloadFlow_ReturnsFile()
    {
        // Arrange
        var submissionId = Guid.Parse("4f7885dd-3072-40a1-8b26-16c5a66a7526");

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId));
        const string testCsvData = "test,csv,data";
        SetupFacadeMockFileDownload(testCsvData: testCsvData);

        // Act - Step 1: Hit the initial download link (sets session state)
        // This will follow redirects and end up on the download page
        var downloadPageResponse = await Client.GetAsync(
            $"/regulators/submission-file-download?downloadType=OrganisationDetails&submissionId={submissionId}");

        downloadPageResponse.EnsureSuccessStatusCode();
        var html = await downloadPageResponse.Content.ReadAsStringAsync();

        // Verify we're on the download page with the meta-refresh URL
        // This doesn't truly test the download flow as the browser would execute it,
        // but it proves the two sides of the meta-refresh are in place so should catch any regression
        string downloadPageUrl = $"/regulators/RegistrationSubmissions/FileDownloadInProgress?submissionId={submissionId}";
        html.Should().Contain(
            $"""<meta http-equiv="refresh" content="1; url={downloadPageUrl}" />""");

        // Act - Step 2: Hit the FileDownloadInProgress endpoint (simulating meta-refresh)
        // This is a separate request that should use the session state from step 1
        var downloadResponse = await Client.GetAsync(
            downloadPageUrl);

        // Assert - should return the actual file
        var responseBody = await downloadResponse.Content.ReadAsStringAsync();
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            $"FileDownloadInProgress should return 200 OK with the file content. Got: {responseBody[..Math.Min(500, responseBody.Length)]}");
        downloadResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/octet-stream",
            $"Expected file download but got HTML. Session state may not be persisting. Response: {responseBody[..Math.Min(200, responseBody.Length)]}");

        responseBody.Should().Be(testCsvData,
            "The downloaded file should contain the mock data from the facade");
    }

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupFacadeMockFileDownload(string testCsvData) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/api/downloads/file-download"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/octet-stream")
                .WithHeader("Content-Disposition", "attachment; filename=\"test.csv\"")
                .WithBody(testCsvData));
}
