namespace EPR.RegulatorService.Frontend.Core.DTOs
{
    using EPR.RegulatorService.Frontend.Core.Enums;

    public class RegistrationSubmissionDetailsDto
    {
        public Guid SubmissionId { get; set; }
        public Guid OrganisationId { get; set; }
        public string OrganisationReference { get; set; }
        public string OrganisationName { get; set; }
        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }
        public int NationId { get; set; }
        public string NationCode { get; set; }
        public int RelevantYear { get; set; }
        public DateTime SubmissionDate { get; set; }
        public RegistrationSubmissionStatus SubmissionStatus { get; set; }
        public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }
        public DateTime? StatusPendingDate { get; set; }
        public string? RegulatorComments { get; set; } = string.Empty;
        public string? ProducerComments { get; set; } = string.Empty;
        public string ApplicationReferenceNumber { get; set; } = string.Empty;
        public string? RegistrationReferenceNumber { get; set; } = string.Empty;
        public string CompaniesHouseNumber { get; set; }
        public string? BuildingName { get; set; }
        public string? SubBuildingName { get; set; }
        public string? BuildingNumber { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string? DependentLocality { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }

        public SubmissionDetailsDto SubmissionDetails { get; set; }
        public DateTime? RegulatorResubmissionDecisionDate { get; set; }
        public DateTime? RegulatorDecisionDate { get; set; }
        public DateTime? ProducerCommentDate { get; set; }
        public Guid? RegulatorUserId { get; set; }
        public bool IsOnlineMarketPlace { get; set; }
        public int NumberOfSubsidiaries { get; set; }
        public int NumberOfOnlineSubsidiaries { get; set; }
        public bool IsLateSubmission { get; set; }
        public string OrganisationSize { get; set; }
        public bool IsComplianceScheme { get; set; }
        public string SubmissionPeriod { get; set; }

        public List<CsoMembershipDetailsDto> CsoMembershipDetails { get; set; }
        public bool IsResubmission { get; set; }
        public string ResubmissionFileId { get; set; }
    }
}
