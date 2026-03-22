namespace IntegrationTests.Builders;

public class RegistrationSubmissionDetailsBuilder
{
    public Guid SubmissionId { get; private set; }
    private string _organisationName = "Test Organisation";
    private string _organisationType = "large";
    private int _relevantYear = 2025;
    private string _submissionDate = "2025-01-10T09:30:00Z";
    private string _submissionStatus = "Pending";
    private bool _isResubmission = false;
    private string? _resubmissionFileId = null;
    private string? _resubmissionDate = null;
    private bool _isComplianceScheme = true;
    private string _registrationJourneyType = "CsoLargeProducer";
    private string? _organisationSize = null;

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

    public RegistrationSubmissionDetailsBuilder WithIsResubmission(bool isResubmission, string? resubmissionFileId = null, string? resubmissionDate = null)
    {
        _isResubmission = isResubmission;
        _resubmissionFileId = resubmissionFileId;
        _resubmissionDate = resubmissionDate ?? "2025-06-15T10:00:00Z";
        return this;
    }

    public RegistrationSubmissionDetailsBuilder AsProducer(string organisationSize = "Large")
    {
        _isComplianceScheme = false;
        _registrationJourneyType = organisationSize == "Small" ? "DirectSmallProducer" : "DirectLargeProducer";
        _organisationSize = organisationSize;
        return this;
    }

    public object Build() => new
    {
        submissionId = SubmissionId,
        organisationId = Guid.NewGuid().ToString(),
        organisationReference = "100001",
        organisationName = _organisationName,
        organisationType = _organisationType,
        registrationJourneyType = _registrationJourneyType,
        nationId = 1,
        nationCode = "GB-ENG",
        relevantYear = _relevantYear,
        submissionDate = _submissionDate,
        submissionStatus = _submissionStatus,
        resubmissionStatus = _isResubmission ? "Pending" : (string?)null,
        resubmissionDate = _resubmissionDate,
        statusPendingDate = (string?)null,
        isResubmission = _isResubmission,
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
            timeAndDateOfResubmission = _resubmissionDate,
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
                    blobName = "B1C2D3E4-F5A6-7890-ABCD-EF1234567890",
                    downloadType = "OrganisationDetails"
                }
            },
            submissionPeriod = $"January to December {_relevantYear}",
            accountRoleId = 1,
            submittedBy = "Test User",
            resubmissionStatus = _isResubmission ? "Pending" : (string?)null,
            registrationDate = (string?)null,
            resubmissionDate = _resubmissionDate,
            isResubmission = _isResubmission,
            resubmissionFileId = _resubmissionFileId
        },
        regulatorDecisionDate = (string?)null,
        regulatorResubmissionDecisionDate = (string?)null,
        producerCommentDate = (string?)null,
        regulatorUserId = (string?)null,
        isOnlineMarketPlace = false,
        numberOfSubsidiaries = 0,
        numberOfOnlineSubsidiaries = 0,
        isLateSubmission = true,
        organisationSize = _organisationSize,
        isComplianceScheme = _isComplianceScheme,
        submissionPeriod = $"January to December {_relevantYear}",
        csoMembershipDetails = _isComplianceScheme ? new[]
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
        } : Array.Empty<object>(),
        resubmissionFileId = _resubmissionFileId
    };
}
