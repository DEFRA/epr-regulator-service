namespace EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    public class RegistrationSubmissionDetailsViewModel
    {
        public Guid SubmissionId { get; set; }
        public Guid OrganisationId { get; set; }
        public bool IsResubmission { get; set; }

        public string OrganisationName { get; set; }
        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }
        public string OrganisationReference { get; set; }
        public string ProducerRegistrationNumber { get; set; } = string.Empty;
        public string? ApplicationReferenceNumber { get; set; } = string.Empty;

        public int NationId { get; set; }
        public string NationCode { get; set; }
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

        public string CompaniesHouseNumber { get; set; }
        public string PowerBiLogin { get; set; }

        public RegistrationSubmissionStatus SubmissionStatus { get; set; }
        public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }

        public SubmissionDetailsViewModel SubmissionDetails { get; set; }

        public PaymentDetailsViewModel PaymentDetails { get; set; }

        public string? RegulatorComments { get; set; } = string.Empty;
        public string? ProducerComments { get; set; } = string.Empty;  
    }
}
