namespace EPR.RegulatorService.Frontend.Web.Mappings.ManageRegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.DTOs;
    using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

    public static class RegistrationSubmissionDetailsModelMapper
    {
        public static RegistrationSubmissionDetailsViewModel MapToViewModel(RegistrationSubmissionDetailsDto dto) => new()
        {
            SubmissionId = dto.SubmissionId,
            OrganisationId = dto.OrganisationId,
            OrganisationReference = dto.OrganisationReference,
            OrganisationName = dto.OrganisationName,
            OrganisationType = dto.OrganisationType,
            NationId = dto.NationId,
            NationCode = dto.NationCode,
            RelevantYear = dto.RelevantYear,
            SubmissionDate = dto.SubmissionDate,
            SubmissionStatus = dto.SubmissionStatus,
            ResubmissionStatus = dto.ResubmissionStatus,
            StatusPendingDate = dto.StatusPendingDate,
            RegulatorComments = dto.RegulatorComments,
            ProducerComments = dto.ProducerComments,
            ApplicationReferenceNumber = dto.ApplicationReferenceNumber,
            RegistrationReferenceNumber = dto.RegistrationReferenceNumber,
            CompaniesHouseNumber = dto.CompaniesHouseNumber,
            BuildingName = dto.BuildingName,
            SubBuildingName = dto.SubBuildingName,
            BuildingNumber = dto.BuildingNumber,
            Street = dto.Street,
            Locality = dto.Locality,
            DependentLocality = dto.DependentLocality,
            Town = dto.Town,
            County = dto.County,
            Country = dto.Country,
            Postcode = dto.Postcode
        };

        public static RegistrationSubmissionDetailsDto MapToDto(RegistrationSubmissionDetailsViewModel viewModel)
        {
            return new RegistrationSubmissionDetailsDto
            {

            };
        }
    }
}
