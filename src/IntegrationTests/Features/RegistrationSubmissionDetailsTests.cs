namespace IntegrationTests.Features;

using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;

using Builders;

using Infrastructure;
using PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

[Collection(SequentialCollection.Sequential)]
public class RegistrationSubmissionDetailsTests : IntegrationTestBase
{
    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        SetupUserAccountsMock();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShowsOrganisationDetailFromFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("0163A629-7780-445F-B00E-1898546BDF0C");

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("Compliance Scheme Ltd")
                .WithOrganisationType("compliance")
                .WithRelevantYear(2025));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.OrganisationName.Should().Be("Compliance Scheme Ltd");
            detailsPage.OrganisationType.Should().Contain("Compliance Scheme");
            detailsPage.RelevantYear.Should().Be(2025);
        }
    }

    [Fact]
    public async Task ShowsPaymentDetailsFromPaymentFacade()
    {
        // Arrange
        var submissionId = Guid.Parse("1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e");

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationName("CSO Large Producer Ltd")
                .WithOrganisationType("compliance")
                .WithRelevantYear(2025));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(262000)
                .WithTotalFee(285400)
                .WithPreviousPayment(50000)
                .WithOutstandingPayment(235400));

        // Act
        var detailsPage = await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            requestUri: $"/regulators/registration-submission-details/{submissionId}");

        // Assert
        using (new AssertionScope())
        {
            detailsPage.PaymentDetails.Should().NotBeNull();
            detailsPage.PaymentDetails!.HasPaymentSection.Should().BeTrue();
            detailsPage.ApplicationFee.Should().Be(2620.00m);
            detailsPage.SubTotal.Should().Be(2854.00m);
            detailsPage.PreviousPaymentReceived.Should().Be(500.00m);
            detailsPage.TotalOutstanding.Should().Be(2354.00m);
        }
    }

    [Fact]
    public async Task ComplianceSchemeRegistrationFeeRequest_IncludesNumberOfSubsidiariesClosedLoopRecyclingFromFacade()
    {
        var submissionId = Guid.NewGuid();
        const string appRef = "REG-INT-CSO-CL-001";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .WithOrganisationType("compliance")
                .WithRegistrationJourneyType("CsoLargeProducer")
                .WithApplicationReferenceNumber(appRef)
                .WithCsoMemberClosedLoopRecycling(8));

        SetupPaymentFacadeMockComplianceSchemeRegistrationFee(
            CompliancePaymentResponseBuilder.Default()
                .WithComplianceSchemeRegistrationFee(100000)
                .WithTotalFee(100000));

        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        var requestBody = FindLastRegistrationFeePostBody(FacadeServer, "compliance-scheme/registration-fee", appRef);
        requestBody.Should().NotBeNull();

        using var doc = JsonDocument.Parse(requestBody!);
        var members = GetJsonPropertyCaseInsensitive(doc.RootElement, "complianceSchemeMembers");
        members.ValueKind.Should().Be(JsonValueKind.Array);
        var firstMember = members.EnumerateArray().First();
        GetJsonPropertyCaseInsensitive(firstMember, "memberType").GetString().Should().Be("large");
        GetJsonPropertyCaseInsensitive(firstMember, "noOfSubsidiariesClosedLoopRecycling").GetInt32().Should().Be(8);
        GetJsonPropertyCaseInsensitive(firstMember, "isClosedLoopRecycling").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ProducerRegistrationFeeRequest_IncludesNumberOfSubsidiariesClosedLoopRecyclingFromFacade()
    {
        var submissionId = Guid.NewGuid();
        const string appRef = "REG-INT-DIRECT-CL-001";

        SetupFacadeMockRegistrationSubmissionDetails(
            RegistrationSubmissionDetailsBuilder.Default(submissionId)
                .AsDirectLargeProducer()
                .WithApplicationReferenceNumber(appRef)
                .WithProducerClosedLoopRecycling(5));

        SetupPaymentFacadeMockProducerRegistrationFee();

        await GetAsPageModel<ManageRegistrationSubmissionDetailsPageModel>(
            $"/regulators/registration-submission-details/{submissionId}");

        var requestBody = FindLastRegistrationFeePostBody(FacadeServer, "producer/registration-fee", appRef);
        requestBody.Should().NotBeNull();

        using var doc = JsonDocument.Parse(requestBody!);
        GetJsonPropertyCaseInsensitive(doc.RootElement, "producerType").GetString().Should().Be("large");
        GetJsonPropertyCaseInsensitive(doc.RootElement, "noOfSubsidiariesClosedLoopRecycling").GetInt32().Should().Be(5);
        GetJsonPropertyCaseInsensitive(doc.RootElement, "isClosedLoopRecycling").GetBoolean().Should().BeTrue();
    }

    private void SetupPaymentFacadeMockComplianceSchemeRegistrationFee(CompliancePaymentResponseBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/compliance-scheme/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private void SetupPaymentFacadeMockProducerRegistrationFee() =>
        FacadeServer.Given(Request.Create()
                .UsingPost()
                .WithPath("/producer/registration-fee"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    producerRegistrationFee = 165800m,
                    producerLateRegistrationFee = 0m,
                    producerOnlineMarketPlaceFee = 0m,
                    producerClosedLoopRecyclingFee = 0m,
                    previousPayment = 0m,
                    subsidiariesFee = 0m,
                    totalFee = 165800m,
                    outstandingPayment = 165800m,
                    subsidiariesFeeBreakdown = new
                    {
                        totalSubsidiariesOMPFees = 0m,
                        countOfOMPSubsidiaries = 0
                    }
                })));

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    /// <summary>
    /// Returns the JSON request body from the most recent POST to the payment-facade WireMock stub
    /// whose path contains <paramref name="pathSubstring"/> and whose body includes
    /// <paramref name="applicationReferenceSnippet"/> (typically the application reference number).
    /// Used to assert paycal parameters sent outbound during a page render, without parsing HTML.
    /// </summary>
    /// <param name="server">WireMock server that records outbound HTTP calls from the app under test.</param>
    /// <param name="pathSubstring">Fragment of the request path, e.g. <c>producer/registration-fee</c>.</param>
    /// <param name="applicationReferenceSnippet">Text that must appear in the POST body to identify the correct request.</param>
    /// <returns>The raw JSON body, or <see langword="null"/> if no matching request was logged.</returns>
    private static string? FindLastRegistrationFeePostBody(WireMockServer server, string pathSubstring, string applicationReferenceSnippet)
    {
        var entries = server.LogEntries.ToList();
        for (var i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            if (!string.Equals(e.RequestMessage.Method, "post", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (e.RequestMessage.Path is not { } path || !path.Contains(pathSubstring, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var body = e.RequestMessage.Body;
            if (string.IsNullOrEmpty(body))
            {
                body = e.RequestMessage.BodyData?.BodyAsString;
            }

            if (!string.IsNullOrEmpty(body) && body.Contains(applicationReferenceSnippet, StringComparison.Ordinal))
            {
                return body;
            }
        }

        return null;
    }

    private static JsonElement GetJsonPropertyCaseInsensitive(JsonElement parent, string name)
    {
        foreach (var p in parent.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return p.Value;
            }
        }

        throw new InvalidOperationException($"Missing JSON property '{name}'.");
    }
}
