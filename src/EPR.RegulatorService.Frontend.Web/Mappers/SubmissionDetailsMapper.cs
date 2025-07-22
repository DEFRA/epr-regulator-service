namespace EPR.RegulatorService.Frontend.Web.Mappers;

using Constants;

using Core.Models.RegistrationSubmissions;

using ViewModels.RegistrationSubmissions;

public static class SubmissionDetailsMapper
{
    public static SubmissionDetailsViewModel.FileDetails FromRegistrationFileDetails(
        RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails otherFile)
    {
        if (otherFile is null)
            return null;

        return new SubmissionDetailsViewModel.FileDetails
        {
            Type = otherFile.Type,
            DownloadType = otherFile.Type switch
            {
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company => FileDownloadTypes
                    .OrganisationDetails,
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.partnership => FileDownloadTypes
                    .PartnershipDetails,
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.brands => FileDownloadTypes
                    .BrandDetails,
                _ => ""
            },
            Label = otherFile.Type switch
            {
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.company =>
                    "SubmissionDetails.OrganisationDetails",
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.partnership =>
                    "SubmissionDetails.PartnerDetails",
                RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileType.brands =>
                    "SubmissionDetails.BrandDetails",
                _ => ""
            },
            FileName = otherFile.FileName,
            FileId = otherFile.FileId,
            BlobName = otherFile.BlobName
        };
    }

    public static RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails ToRegistrationFileDetails(
        SubmissionDetailsViewModel.FileDetails fileDetails)
    {
        if (fileDetails is null)
            return null;

        return new RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails
        {
            Type = fileDetails.Type,
            FileName = fileDetails.FileName,
            FileId = fileDetails.FileId,
            BlobName = fileDetails.BlobName
        };
    }

    public static RegistrationSubmissionOrganisationSubmissionSummaryDetails ToRegistrationSubmissionDetails(
        SubmissionDetailsViewModel viewModel)
    {
        if (viewModel is null)
        {
            return null;
        }

        var result = new RegistrationSubmissionOrganisationSubmissionSummaryDetails
        {
            AccountRoleId = viewModel.AccountRoleId,
            Telephone = viewModel.Telephone,
            Email = viewModel.Email,
            DeclaredBy = viewModel.DeclaredBy,
            SubmittedBy = viewModel.SubmittedBy,
            DecisionDate = viewModel.LatestDecisionDate,
            ResubmissionDecisionDate = viewModel.ResubmissionDecisionDate,
            Status = viewModel.Status,
            ResubmissionStatus = viewModel.ResubmissionStatus,
            SubmittedOnTime = viewModel.SubmittedOnTime,
            StatusPendingDate = viewModel.StatusPendingDate,
            TimeAndDateOfSubmission = viewModel.TimeAndDateOfSubmission,
            TimeAndDateOfResubmission = viewModel.TimeAndDateOfResubmission,
            RegistrationDate = viewModel.RegistrationDate,
            IsResubmission = viewModel.IsResubmission,
            ResubmissionFileId = viewModel.ResubmissionFileId
        };

        if (viewModel.Files != null)
        {
            result.Files = viewModel.Files
                .Select(file => ToRegistrationFileDetails(file))
                .ToList();
        }

        return result;
    }

    public static SubmissionDetailsViewModel FromRegistrationSubmissionDetails(
        RegistrationSubmissionOrganisationSubmissionSummaryDetails details)
    {
        if (details is null)
        {
            return null;
        }

        return new SubmissionDetailsViewModel
        {
            AccountRoleId = details.AccountRoleId,
            AccountRole = GetAccountRole(details.AccountRoleId),
            Telephone = details.Telephone,
            Email = details.Email,
            DeclaredBy = details.DeclaredBy,
            SubmittedBy = details.SubmittedBy,
            LatestDecisionDate = details.DecisionDate,
            ResubmissionDecisionDate = details.ResubmissionDecisionDate,
            Status = details.Status,
            ResubmissionStatus = details.ResubmissionStatus,
            StatusPendingDate = details.StatusPendingDate,
            SubmittedOnTime = details.SubmittedOnTime,
            TimeAndDateOfSubmission = details.TimeAndDateOfSubmission,
            TimeAndDateOfResubmission = details.TimeAndDateOfResubmission,
            RegistrationDate = details.RegistrationDate,
            Files = details.Files?.Select(file => FromRegistrationFileDetails(file)).ToList() ??
                    new List<SubmissionDetailsViewModel.FileDetails>(),
            IsResubmission = details.IsResubmission,
            ResubmissionFileId = details.ResubmissionFileId
        };
    }

    private static Core.Enums.ServiceRole? GetAccountRole(int? serviceRoleId)
    {
        var retVal = serviceRoleId.HasValue ? (Core.Enums.ServiceRole)serviceRoleId : (Core.Enums.ServiceRole?)null;

        return retVal;
    }
}