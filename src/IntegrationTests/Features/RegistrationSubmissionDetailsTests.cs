namespace IntegrationTests.Features;

using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Infrastructure;
using MockRegulatorFacade.FacadeApi;
using PageModels;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

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

    private void SetupFacadeMockRegistrationSubmissionDetails(RegistrationSubmissionDetailsBuilder builder) =>
        FacadeServer.Given(Request.Create()
                .UsingGet()
                .WithPath($"/api/organisation-registration-submission-details/{builder.SubmissionId}"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(builder.Build())));

    private class RegistrationSubmissionDetailsBuilder
    {
        public Guid SubmissionId { get; private set; }
        private string _organisationName = "Test Organisation";
        private string _organisationType = "large";
        private int _relevantYear = 2025;
        private string _submissionDate = "2025-01-10T09:30:00Z";
        private string _submissionStatus = "Pending";

        private RegistrationSubmissionDetailsBuilder(Guid submissionId)
        {
            SubmissionId = submissionId;
        }

        public static RegistrationSubmissionDetailsBuilder Default(Guid submissionId) => new(submissionId);

        public RegistrationSubmissionDetailsBuilder WithOrganisationName(string organisationName)
        {
            _organisationName = organisationName;
            return this;
        }

        public RegistrationSubmissionDetailsBuilder WithOrganisationType(string organisationType)
        {
            _organisationType = organisationType;
            return this;
        }

        public RegistrationSubmissionDetailsBuilder WithRelevantYear(int relevantYear)
        {
            _relevantYear = relevantYear;
            return this;
        }

        public RegistrationSubmissionDetailsBuilder WithSubmissionDate(string submissionDate)
        {
            _submissionDate = submissionDate;
            return this;
        }

        public RegistrationSubmissionDetailsBuilder WithSubmissionStatus(string submissionStatus)
        {
            _submissionStatus = submissionStatus;
            return this;
        }

        public object Build() => new
        {
            submissionId = SubmissionId,
            organisationId = Guid.NewGuid().ToString(),
            organisationReference = "100001",
            organisationName = _organisationName,
            organisationType = _organisationType,
            registrationJourneyType = "CsoLargeProducer",
            nationId = 1,
            nationCode = "GB-ENG",
            relevantYear = _relevantYear,
            submissionDate = _submissionDate,
            submissionStatus = _submissionStatus,
            resubmissionStatus = "Pending",
            resubmissionDate = (string?)null,
            statusPendingDate = (string?)null,
            isResubmission = false,
            registrationDate = (string?)null,
            regulatorComments = "",
            producerComments = "Test comment",
            applicationReferenceNumber = "REG-2025-001",
            registrationReferenceNumber = "",
            companiesHouseNumber = "CS123456",
            buildingName = "Test Building",
            subBuildingName = "1",
            buildingNumber = "1",
            street = "Test Street",
            locality = "Test Locality",
            dependentLocality = "2",
            town = "Test Town",
            county = "Test County",
            country = "United Kingdom",
            postcode = "SW1A 1AA",
            submissionDetails = new
            {
                status = _submissionStatus,
                decisionDate = (string?)null,
                resubmissionDecisionDate = (string?)null,
                statusPendingDate = (string?)null,
                timeAndDateOfSubmission = _submissionDate,
                submittedOnTime = false,
                submittedByUserId = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
                accountRole = "Approved Person",
                telephone = "01234567890",
                email = "test.user@example.com",
                declaredBy = "Not required (compliance scheme)",
                files = new[]
                {
                    new
                    {
                        type = "company",
                        fileId = "F1A2B3C4-D5E6-7890-ABCD-EF1234567890",
                        fileName = "reg-20250110_093000.csv",
                        blobName = "B1C2D3E4-F5A6-7890-ABCD-EF1234567890"
                    }
                },
                submissionPeriod = $"January to December {_relevantYear}",
                accountRoleId = 1,
                submittedBy = "Test User",
                resubmissionStatus = (string?)null,
                registrationDate = (string?)null,
                resubmissionDate = (string?)null,
                isResubmission = false,
                resubmissionFileId = (string?)null
            },
            regulatorDecisionDate = (string?)null,
            regulatorResubmissionDecisionDate = (string?)null,
            producerCommentDate = (string?)null,
            regulatorUserId = (string?)null,
            isOnlineMarketPlace = false,
            numberOfSubsidiaries = 0,
            numberOfOnlineSubsidiaries = 0,
            isLateSubmission = true,
            organisationSize = (string?)null,
            isComplianceScheme = true,
            submissionPeriod = $"January to December {_relevantYear}",
            csoMembershipDetails = new[]
            {
                new
                {
                    memberId = "100001",
                    memberType = "large",
                    isOnlineMarketPlace = false,
                    isLateFeeApplicable = true,
                    numberOfSubsidiaries = 0,
                    NumberOfSubsidiariesOnlineMarketPlace = 0,
                    relevantYear = _relevantYear,
                    submittedDate = _submissionDate,
                    submissionPeriodDescription = $"January to December {_relevantYear}"
                }
            },
            resubmissionFileId = (string?)null
        };
    }
}
