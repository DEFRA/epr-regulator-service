namespace IntegrationTests.Features;

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;
using EPR.RegulatorService.Frontend.Web.Constants;
using IntegrationTests.Builders;
using IntegrationTests.Infrastructure;
using IntegrationTests.PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

[Collection(SequentialCollection.Sequential)]
public class SubmissionsTests : IntegrationTestBase
{
    [Theory]
    [InlineData(null, "Ignored user org name", 1, "Environment Agency", "Manage packaging data submissions")]
    [InlineData("cy", "Ignored user org name", 1, "Asiantaeth yr Amgylchedd", "Rheoli cyflwyniadau data pecynwaith")]
    [InlineData(null, "Ignored user org name", 4, "Natural Resources Wales", "Manage packaging data submissions")]
    [InlineData("cy", "Ignored user org name", 4, "Cyfoeth Naturiol Cymru", "Rheoli cyflwyniadau data pecynwaith")]
    public async Task SubmissionsPage_ShowsDynamicOrganisationCaption(
        string? culture,
        string organisationName,
        int nationId,
        string expectedCaption,
        string expectedHeading)
    {
        // Arrange
        SetupUserAccountsMock(UserAccountBuilder.Default()
            .WithOrganisationName(organisationName)
            .WithNationId(nationId));
        SetupFacadeMockPomSubmissions([]);
        await SetLanguage(culture);

        // Act
        var page = await GetAsPageModel<SubmissionsPageModel>(
            requestUri: "/regulators/manage-packaging-data-submissions");

        // Assert
        using (new AssertionScope())
        {
            page.AgencyNameCaption.Should().Be(expectedCaption);
            page.PageHeading.Should().Be(expectedHeading);
        }
    }

    /// <summary>
    /// Smoke test: model binding must accept <c>jsonSubmission</c> from the POST body (hidden field),
    /// not only from the query string — see AMCR-147 / WAF.
    /// </summary>
    [Fact]
    public async Task Submissions_Post_WithJsonSubmissionInFormBody_RedirectsToSubmissionDetails()
    {
        SetupUserAccountsMock();
        SetupFacadeMockPomSubmissions([]);

        var submission = CreateSampleSubmission();
        var json = JsonSerializer.Serialize(submission);

        using var client = CreateNoRedirectClient();
        await client.GetAsync("/regulators/manage-packaging-data-submissions");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["jsonSubmission"] = json,
        });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var postUri = "/regulators/manage-packaging-data-submissions";
        postUri.Should().NotContain("jsonSubmission", "POST URL must not carry jsonSubmission in the query string");

        using var response = await client.PostAsync(postUri, content);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("submissionHash", "expected redirect to submission details with hash");
        response.Headers.Location!.ToString().Should().Contain(PagePath.SubmissionDetails);
    }

    private static Submission CreateSampleSubmission() =>
        new()
        {
            SubmissionId = Guid.Parse("a3f6c7b8-9d4e-4f9a-bcde-1234567890ab"),
            SubmittedDate = new DateTime(2023, 10, 30, 10, 51, 23, DateTimeKind.Utc),
            Decision = SubmissionStatus.Pending,
            IsResubmission = false,
            IsResubmissionRequired = false,
            Comments = string.Empty,
            OrganisationId = Guid.Parse("d4e5f6a7-b8c9-0d1e-2f3a-9876543210cd"),
            OrganisationName = "Integration Test Org Ltd",
            OrganisationType = OrganisationType.DirectProducer,
            OrganisationReference = "REF123",
            UserId = Guid.Parse("e1e2e3e4-f5a6-47b8-9c0d-1e2f3a4b5c6d"),
            Email = "regulator@test.example",
            FirstName = "Test",
            LastName = "Regulator",
            Telephone = "01234567890",
            ServiceRole = "Approved person",
            FileId = Guid.Parse("f1f2f3f4-a5b6-47c8-9d0e-1f2a3b4c5d6e"),
            PomBlobName = "blob-name",
            PomFileName = "pom.csv",
        };

    private void SetupFacadeMockPomSubmissions(object[] data) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath("/api/pom/get-submissions"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    items = data,
                    currentPage = 1,
                    pageSize = 20,
                    totalItems = data.Length,
                })));
}
