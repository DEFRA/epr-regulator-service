namespace IntegrationTests.Builders;

public class RegistrationSubmissionDetailsBuilder
{
    public Guid SubmissionId { get; private set; }
    private string _organisationName = "Test Organisation";
    private string _organisationType = "large";
    private int _relevantYear = 2025;
    private string _submissionDate = "2025-01-10T09:30:00Z";
    private string _submissionStatus = "Pending";
    private string _registrationJourneyType = "CsoLargeProducer";
    private bool _isComplianceScheme = true;
    private bool _emptyCsoMembershipDetails;
    private string? _organisationSize;
    private int _numberOfSubsidiariesClosedLoopRecyclingRoot;
    private bool _isClosedLoopRecyclingRoot;
    private bool _csoMemberIsClosedLoopRecycling;
    private int _csoMemberNoOfSubsidiariesClosedLoopRecycling;
    private string _applicationReferenceNumber = "REG-2025-001";

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

    public RegistrationSubmissionDetailsBuilder WithApplicationReferenceNumber(string applicationReferenceNumber)
    {
        _applicationReferenceNumber = applicationReferenceNumber;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithRegistrationJourneyType(string registrationJourneyType)
    {
        _registrationJourneyType = registrationJourneyType;
        return this;
    }

    /// <summary>Direct large producer: compliance scheme payment component is not used; producer payment API is called instead.</summary>
    public RegistrationSubmissionDetailsBuilder AsDirectLargeProducer()
    {
        _organisationType = "large";
        _registrationJourneyType = "DirectLargeProducer";
        _isComplianceScheme = false;
        _emptyCsoMembershipDetails = true;
        _organisationSize = "large";
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithProducerClosedLoopRecycling(int noOfSubsidiariesClosedLoopRecycling, bool isClosedLoopRecycling = true)
    {
        _numberOfSubsidiariesClosedLoopRecyclingRoot = noOfSubsidiariesClosedLoopRecycling;
        _isClosedLoopRecyclingRoot = isClosedLoopRecycling;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithCsoMemberClosedLoopRecycling(int noOfSubsidiariesClosedLoopRecycling, bool memberIsClosedLoopRecycling = true)
    {
        _csoMemberNoOfSubsidiariesClosedLoopRecycling = noOfSubsidiariesClosedLoopRecycling;
        _csoMemberIsClosedLoopRecycling = memberIsClosedLoopRecycling;
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
        resubmissionStatus = "Pending",
        resubmissionDate = (string?)null,
        statusPendingDate = (string?)null,
        isResubmission = false,
        registrationDate = (string?)null,
        regulatorComments = "",
        producerComments = "Test comment",
        applicationReferenceNumber = _applicationReferenceNumber,
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
                    blobName = "B1C2D3E4-F5A6-7890-ABCD-EF1234567890",
                    downloadType = "OrganisationDetails"
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
        numberOfSubsidiariesClosedLoopRecycling = _numberOfSubsidiariesClosedLoopRecyclingRoot,
        isLateSubmission = true,
        IsClosedLoopRecycling = _isClosedLoopRecyclingRoot,
        organisationSize = _organisationSize,
        isComplianceScheme = _isComplianceScheme,
        submissionPeriod = $"January to December {_relevantYear}",
        csoMembershipDetails = _emptyCsoMembershipDetails
            ? Array.Empty<object>()
            : new object[]
            {
                new
                {
                    memberId = "100001",
                    memberType = "large",
                    isOnlineMarketPlace = false,
                    isLateFeeApplicable = true,
                    isClosedLoopRecycling = _csoMemberIsClosedLoopRecycling,
                    NumberOfSubsidiariesClosedLoopRecycling = _csoMemberNoOfSubsidiariesClosedLoopRecycling,
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
