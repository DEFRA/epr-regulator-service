namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

    public class RegistrationSubmissionDetailsViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationReference { get; set; }

        public string OrganisationName { get; set; }

        public string? ApplicationReferenceNumber { get; set; }

        public string? RegistrationReferenceNumber { get; set; }

        public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

        public BusinessAddress BusinessAddress { get; set; }

        public string CompaniesHouseNumber { get; set; }

        public string RegisteredNation { get; set; }

        public string PowerBiLogin { get; set; }

        public RegistrationSubmissionStatus Status { get; set; }

        public SubmissionDetailsViewModel SubmissionDetails { get; set; }

        public PaymentDetailsViewModel PaymentDetails { get; set; }

        public DateTime RegistrationDateTime { get; set; }

        // Implicit operator from RegistrationSubmissionOrganisationDetails to RegistrationSubmissionDetailsViewModel
        public static implicit operator RegistrationSubmissionDetailsViewModel(RegistrationSubmissionOrganisationDetails details)
        {
            return new RegistrationSubmissionDetailsViewModel
            {
                OrganisationId = details.OrganisationID,
                OrganisationReference = details.OrganisationReference.Substring(0, 10),
                OrganisationName = details.OrganisationName,
                ApplicationReferenceNumber = details.ApplicationReferenceNumber,
                RegistrationReferenceNumber = details.RegistrationReferenceNumber,
                OrganisationType = details.OrganisationType,
                CompaniesHouseNumber = details.CompaniesHouseNumber,
                RegisteredNation = details.Country, // Assuming RegisteredNation corresponds to the Country
                Status = details.RegistrationStatus,
                RegistrationDateTime = details.RegistrationDateTime,
                BusinessAddress = new BusinessAddress
                {
                    BuildingName = details.BuildingName,
                    SubBuildingName = details.SubBuildingName,
                    BuildingNumber = details.BuildingNumber,
                    Street = details.Street,
                    Town = details.Town,
                    County = details.County,
                    Country = details.Country,
                    PostCode = details.Postcode
                }
            };
        }

        public string? ProducerComments { get; set; } 

        public string? RegulatorComments { get; set; }  
    }
}