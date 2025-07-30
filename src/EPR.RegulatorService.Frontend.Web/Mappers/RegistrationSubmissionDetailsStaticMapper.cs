namespace EPR.RegulatorService.Frontend.Web.Mappers
{
    using EPR.RegulatorService.Frontend.Core.Models;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

    public static class RegistrationSubmissionDetailsStaticMapper 
    {
        public static RegistrationSubmissionDetailsViewModel? MapFromOrganisationDetails(RegistrationSubmissionOrganisationDetails? details) => details is null
                ? null
                : new RegistrationSubmissionDetailsViewModel
                {
                    SubmissionId = details.SubmissionId,
                    OrganisationId = details.OrganisationId,
                    OrganisationReference = details.OrganisationReference[..Math.Min(details.OrganisationReference.Length, 10)],
                    OrganisationName = details.OrganisationName,
                    ReferenceNumber = details.ApplicationReferenceNumber,
                    RegistrationReferenceNumber = details.RegistrationReferenceNumber,
                    OrganisationType = details.OrganisationType,
                    CompaniesHouseNumber = details.CompaniesHouseNumber,
                    RegisteredNation = details.Country, // Assuming RegisteredNation corresponds to the Country
                    NationId = details.NationId,
                    NationCode = details.NationCode,
                    Status = details.SubmissionStatus,
                    ResubmissionStatus = details.ResubmissionStatus,
                    RegistrationDateTime = details.RegistrationDate,
                    RegistrationYear = details.RelevantYear,
                    RegulatorComments = details.RegulatorComments,
                    ProducerComments = details.ProducerComments,
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
                    },
                    SubmissionDetails = details.SubmissionDetails,
                    RejectReason = details.RejectReason,
                    CancellationReason = details.CancellationReason,
                    ProducerDetails = details.ProducerDetails,
                    CSOMembershipDetails = details.CsoMembershipDetails,
                    IsResubmission = details.IsResubmission,
                    ResubmissionFileId = details.ResubmissionFileId
                };

        public static RegistrationSubmissionOrganisationDetails? MapToOrganisationDetails(RegistrationSubmissionDetailsViewModel? viewModel) => viewModel is null
                ? null
                : new RegistrationSubmissionOrganisationDetails
                {
                    SubmissionId = viewModel.SubmissionId,
                    OrganisationId = viewModel.OrganisationId,
                    OrganisationReference = viewModel.OrganisationReference[..Math.Min(viewModel.OrganisationReference.Length, 10)],
                    OrganisationName = viewModel.OrganisationName,
                    ApplicationReferenceNumber = viewModel.ReferenceNumber,
                    RegistrationReferenceNumber = viewModel.RegistrationReferenceNumber,
                    OrganisationType = viewModel.OrganisationType,
                    CompaniesHouseNumber = viewModel.CompaniesHouseNumber,
                    Country = viewModel.RegisteredNation,
                    SubmissionStatus = viewModel.Status,
                    ResubmissionStatus = viewModel.ResubmissionStatus,
                    RegistrationDate = viewModel.RegistrationDateTime,
                    RelevantYear = viewModel.RegistrationYear,
                    RegulatorComments = viewModel.RegulatorComments,
                    ProducerComments = viewModel.ProducerComments,
                    BuildingName = viewModel.BusinessAddress?.BuildingName,
                    SubBuildingName = viewModel.BusinessAddress?.SubBuildingName,
                    BuildingNumber = viewModel.BusinessAddress?.BuildingNumber,
                    Street = viewModel.BusinessAddress?.Street,
                    Town = viewModel.BusinessAddress?.Town,
                    County = viewModel.BusinessAddress?.County,
                    Postcode = viewModel.BusinessAddress?.PostCode,
                    SubmissionDetails = viewModel.SubmissionDetails,
                    NationCode = viewModel.NationCode,
                    NationId = viewModel.NationId,
                    RejectReason = viewModel.RejectReason,
                    CancellationReason = viewModel.CancellationReason,
                    CsoMembershipDetails = viewModel.CSOMembershipDetails?.ToList(),
                    ProducerDetails = viewModel.ProducerDetails,
                    IsResubmission = viewModel.IsResubmission,
                    ResubmissionFileId = viewModel.ResubmissionFileId,

                };
    }
}
