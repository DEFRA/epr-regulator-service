namespace EPR.RegulatorService.Frontend.Web.Mappings.ManageRegistrationSubmissions
{
    using System.Collections.Generic;
    using System.Globalization;

    using EPR.RegulatorService.Frontend.Core.DTOs;
    using EPR.RegulatorService.Frontend.Core.Enums;
    using EPR.RegulatorService.Frontend.Web.Constants;
    using EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions;

    public static class RegistrationSubmissionDetailsModelMapper
    {
        public static RegistrationSubmissionDetailsViewModel MapToViewModel(RegistrationSubmissionDetailsDto dto) => new()
        {
            SubmissionId = dto.SubmissionId,
            OrganisationId = dto.OrganisationId,
            IsResubmission = dto.IsResubmission,
            OrganisationReference = dto.OrganisationReference[..Math.Min(dto.OrganisationReference.Length, 10)],
            OrganisationName = dto.OrganisationName,
            OrganisationType = dto.OrganisationType,
            SubmissionStatus = dto.SubmissionStatus,
            ResubmissionStatus = dto.ResubmissionStatus,

            NationId = dto.NationId,
            NationCode = dto.NationCode,
            RegulatorComments = dto.RegulatorComments,
            ProducerComments = dto.ProducerComments,
            ApplicationReferenceNumber = dto.ApplicationReferenceNumber,
            ProducerRegistrationNumber = dto.RegistrationReferenceNumber,
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
            Postcode = dto.Postcode,
            SubmissionDetails = MapSubmissionDetails(dto.SubmissionDetails)
        };

        private static SubmissionDetailsViewModel MapSubmissionDetails(SubmissionDetailsDto dto) => new()
        {
            TimeAndDateOfSubmission = dto.TimeAndDateOfSubmission.ToString("dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            TimeAndDateOfResubmission = dto.TimeAndDateOfResubmission?.ToString("dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            ResubmissionDecisionDate = dto.ResubmissionDecisionDate?.ToString("dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            LatestDecisionDate = dto.DecisionDate?.ToString("dd MMMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            SubmittedBy = dto.SubmittedBy,
            AccountRole = dto.AccountRole,
            Telephone = dto.Telephone,
            Email = dto.Email,
            DeclaredBy = dto.DeclaredBy,
            Files = MapFiles(dto.Files)
        };

        public static string DisplayAppropriateSubmissionDate(SubmissionDetailsDto dto)
        {
            const string format = "dd MMMM yyyy HH:mm:ss";

            var targetDate = dto.Status switch
            {
                RegistrationSubmissionStatus.Granted => dto.RegistrationDate,
                RegistrationSubmissionStatus.Refused or
                RegistrationSubmissionStatus.Queried or
                RegistrationSubmissionStatus.Updated or
                RegistrationSubmissionStatus.Accepted or
                RegistrationSubmissionStatus.Rejected => dto.DecisionDate,
                RegistrationSubmissionStatus.Cancelled => dto.StatusPendingDate ?? dto.DecisionDate,
                _ => dto.TimeAndDateOfSubmission,
            } ?? dto.TimeAndDateOfSubmission;

            return targetDate.ToString(dto.Status switch { RegistrationSubmissionStatus.Cancelled => "dd MMMM yyyy", _ => format }, CultureInfo.InvariantCulture);
        }

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
