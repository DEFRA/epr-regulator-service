namespace EPR.RegulatorService.Frontend.Core.DTOs
{
    using EPR.RegulatorService.Frontend.Core.Enums;

    public class RegistrationSubmissionDetailsDto
    {
        public Guid SubmissionId { get; init; }
        public Guid OrganisationId { get; init; }
        public string OrganisationReference { get; init; }
        public string OrganisationName { get; init; }
        public RegistrationSubmissionOrganisationType OrganisationType { get; init; }
        public int NationId { get; init; }
        public string NationCode { get; set; }
        public int RelevantYear { get; init; }
        public DateTime SubmissionDate { get; init; }
        public RegistrationSubmissionStatus SubmissionStatus { get; init; }
        public RegistrationSubmissionStatus? ResubmissionStatus { get; init; }
        public DateTime? StatusPendingDate { get; set; }
        public string? RegulatorComments { get; set; } = string.Empty;
        public string? ProducerComments { get; set; } = string.Empty;
        public string ApplicationReferenceNumber { get; init; } = string.Empty;
        public string? RegistrationReferenceNumber { get; init; } = string.Empty;
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
    }
}
