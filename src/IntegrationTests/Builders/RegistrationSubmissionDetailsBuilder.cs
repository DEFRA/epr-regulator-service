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
    private int _numberOfHoldingCompaniesClosedLoopRecyclingRoot;
    private bool _csoMemberIsClosedLoopRecycling;
    private int _csoMemberNumberOfSubsidiariesClosedLoopRecycling;
    private int _numberOfSubsidiariesRoot;
    private int _csoMemberNumberOfSubsidiaries;
    private string _csoMemberType = "large";
    private string _applicationReferenceNumber = "REG-2025-001";
    private List<CsoMemberBuilder>? _csoMembers;
    private string? _resubmissionFileId = null;
    private string? _resubmissionDate = null;
    private bool _isResubmission;

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

    public RegistrationSubmissionDetailsBuilder WithProducerClosedLoopRecycling(
        int noOfSubsidiariesClosedLoopRecycling,
        int noOfHoldingCompaniesClosedLoopRecycling = 1)
    {
        _numberOfSubsidiariesClosedLoopRecyclingRoot = noOfSubsidiariesClosedLoopRecycling;
        _numberOfHoldingCompaniesClosedLoopRecyclingRoot = noOfHoldingCompaniesClosedLoopRecycling;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithCsoMemberClosedLoopRecycling(
        int noOfSubsidiariesClosedLoopRecycling,
        bool isClosedLoopRecycling = true)
    {
        _csoMemberNumberOfSubsidiariesClosedLoopRecycling = noOfSubsidiariesClosedLoopRecycling;
        _csoMemberIsClosedLoopRecycling = isClosedLoopRecycling;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithOrganisationSize(string organisationSize)
    {
        _organisationSize = organisationSize;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithCsoMemberType(string memberType)
    {
        _csoMemberType = memberType;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithProducerSubsidiaries(int count)
    {
        _numberOfSubsidiariesRoot = count;
        return this;
    }

    public RegistrationSubmissionDetailsBuilder WithCsoMemberSubsidiaries(int count)
    {
        _csoMemberNumberOfSubsidiaries = count;
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

    public RegistrationSubmissionDetailsBuilder WithCsoMembers(params CsoMemberBuilder[] members)
    {
        _csoMembers = members.ToList();
        _emptyCsoMembershipDetails = false;
        _isComplianceScheme = true;
        _organisationType = "compliance";
        _registrationJourneyType = "CsoLargeProducer";
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
        numberOfSubsidiaries = _numberOfSubsidiariesRoot,
        numberOfOnlineSubsidiaries = 0,
        numberOfSubsidiariesClosedLoopRecycling = _numberOfSubsidiariesClosedLoopRecyclingRoot,
        isLateSubmission = true,
        numberOfHoldingCompaniesClosedLoopRecycling = _numberOfHoldingCompaniesClosedLoopRecyclingRoot,
        organisationSize = _organisationSize,
        isComplianceScheme = _isComplianceScheme,
        submissionPeriod = $"January to December {_relevantYear}",
        csoMembershipDetails = BuildCsoMembershipDetails(),
        resubmissionFileId = _resubmissionFileId
    };

    private object[] BuildCsoMembershipDetails()
    {
        if (_emptyCsoMembershipDetails)
        {
            return Array.Empty<object>();
        }

        if (_csoMembers is not null)
        {
            return _csoMembers.Select(m => m.Build(_relevantYear, _submissionDate)).ToArray<object>();
        }

        return
        [
            new
            {
                memberId = "100001",
                memberType = _csoMemberType,
                isOnlineMarketPlace = false,
                isLateFeeApplicable = true,
                isClosedLoopRecycling = _csoMemberIsClosedLoopRecycling,
                numberOfSubsidiariesClosedLoopRecycling = _csoMemberNumberOfSubsidiariesClosedLoopRecycling,
                numberOfSubsidiaries = _csoMemberNumberOfSubsidiaries,
                NumberOfSubsidiariesOnlineMarketPlace = 0,
                relevantYear = _relevantYear,
                submittedDate = _submissionDate,
                submissionPeriodDescription = $"January to December {_relevantYear}"
            }
        ];
    }
}
