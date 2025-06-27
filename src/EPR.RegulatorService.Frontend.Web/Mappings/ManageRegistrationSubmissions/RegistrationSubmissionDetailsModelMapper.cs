namespace EPR.RegulatorService.Frontend.Web.Mappings.ManageRegistrationSubmissions
{
    using System.Collections.Generic;
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Web.Configs;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

    using Microsoft.Extensions.Options;

    public static class RegistrationSubmissionDetailsModelMapper
    {
        const string FullDateFormat = "dd MMMM yyyy HH:mm:ss";
        const string ShortDateFormat = "dd MMMM yyyy";

        public static RegistrationSubmissionDetailsViewModel MapToViewModel(
            RegistrationSubmissionDetailsDto dto,
            IOptions<ExternalUrlsOptions> externalUrlsOptions) => new()
        {
            SubmissionId = dto.SubmissionId,
            OrganisationId = dto.OrganisationId,
            IsResubmission = dto.IsResubmission,
            OrganisationReference = dto.OrganisationReference[..Math.Min(dto.OrganisationReference.Length, 10)],
            OrganisationName = dto.OrganisationName,
            OrganisationType = dto.OrganisationType,
            SubmissionStatus = dto.SubmissionStatus,
            ResubmissionStatus = dto.SubmissionDetails.ResubmissionStatus,

            NationId = dto.NationId,
            NationCode = dto.NationCode,
            RegulatorComments = dto.RegulatorComments,
            ProducerComments = dto.ProducerComments,
            ApplicationReferenceNumber = dto.ApplicationReferenceNumber,
            ProducerRegistrationNumber = dto.RegistrationReferenceNumber,
            CompaniesHouseNumber = dto.CompaniesHouseNumber,
            PowerBiLogin = externalUrlsOptions.Value.PowerBiLogin,

            BuildingName = dto.BuildingName,
            SubBuildingName = dto.SubBuildingName,
            BuildingNumber = dto.BuildingNumber,
            Street = dto.Street,
            Locality = dto.Locality,
            DependentLocality = dto.DependentLocality,
            Town = dto.Town,
            County = dto.County,
            Country = dto.Country,
            Postcode = dto.Postcode,
            SubmissionDetails = MapSubmissionDetails(dto.SubmissionDetails)
        };

        private static SubmissionDetailsViewModel MapSubmissionDetails(SubmissionDetailsDto dto) => new()
        {
            TimeAndDateOfSubmission = FormatDate(dto.TimeAndDateOfSubmission),
            TimeAndDateOfResubmission = FormatNullableDate(dto.TimeAndDateOfResubmission),
            ResubmissionDecisionDate = FormatNullableDate(dto.ResubmissionDecisionDate),
            LatestDecisionDate = MapAppropriateDate(dto),
            SubmittedBy = dto.SubmittedBy,
            AccountRole = dto.AccountRole,
            Telephone = dto.Telephone,
            Email = dto.Email,
            DeclaredBy = dto.DeclaredBy,
            Files = MapFiles(dto.Files)
        };

        private static string MapAppropriateDate(SubmissionDetailsDto dto)
        {
            var selectedDate = dto.Status switch
            {
                RegistrationSubmissionStatus.Granted => dto.RegistrationDate,
                RegistrationSubmissionStatus.Refused or
                RegistrationSubmissionStatus.Queried or
                RegistrationSubmissionStatus.Updated or
                RegistrationSubmissionStatus.Accepted or
                RegistrationSubmissionStatus.Rejected => dto.DecisionDate,
                RegistrationSubmissionStatus.Cancelled => dto.StatusPendingDate ?? dto.DecisionDate,
                _ => null
            };

            var fallbackDate = dto.TimeAndDateOfSubmission;
            var finalDate = selectedDate ?? fallbackDate;

            string format = dto.Status == RegistrationSubmissionStatus.Cancelled ? ShortDateFormat : FullDateFormat;
            return finalDate.ToString(format, CultureInfo.InvariantCulture);
        }

        private static string FormatDate(DateTime date) => date.ToString(FullDateFormat, CultureInfo.InvariantCulture);

        private static string? FormatNullableDate(DateTime? date) => date?.ToString(FullDateFormat, CultureInfo.InvariantCulture);

        private static List<FileDetailsViewModel> MapFiles(List<FileDetailsDto> files)
        {
            var listOfFileDetailsViewModel = new List<FileDetailsViewModel>();

            foreach (var file in files)
            {
                listOfFileDetailsViewModel.Add(
                     new FileDetailsViewModel
                     {
                         FileId = file.FileId,
                         FileName = file.FileName,
                         Type = file.Type,
                         BlobName = file.BlobName,
                         DownloadType = file.Type switch
                         {
                             FileType.company => FileDownloadTypes.OrganisationDetails,
                             FileType.partnership => FileDownloadTypes.PartnershipDetails,
                             FileType.brands => FileDownloadTypes.BrandDetails,
                             _ => ""
                         },
                         Label = file.Type switch
                         {
                             FileType.company => "SubmissionDetails.OrganisationDetails",
                             FileType.partnership => "SubmissionDetails.PartnerDetails",
                             FileType.brands => "SubmissionDetails.BrandDetails",
                             _ => ""
                         },
                     }
                );
            }

            return listOfFileDetailsViewModel;
        }

        public static RegistrationSubmissionDetailsDto MapToDto(RegistrationSubmissionDetailsViewModel viewModel)
        {
            return new RegistrationSubmissionDetailsDto
            {

            };
        }
    }
}
