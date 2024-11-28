namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Globalization;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.RegistrationSubmissionOrganisationSubmissionSummaryDetails;

public class SubmissionDetailsViewModel
{
    public class FileDetails
    {
        FileType Type { get; set; }

        public string Label { get; set; }
        public string FileName { get; set; }
        public string FileId { get; set; }
        public string BlobName { get; set; }

        public string DownloadUrl { get; set; }

        public static implicit operator FileDetails(RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails otherFile) => otherFile is null ? null : new FileDetails
        {
            Type = otherFile.Type,
            Label = "tbc",
            FileName = otherFile.FileName,
            DownloadUrl = "generated",
            FileId = otherFile.FileId,
            BlobName = otherFile.BlobName
        };

        public static implicit operator RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails(FileDetails otherFile) => otherFile is null ? null : new()
        {
            Type = otherFile.Type,
            FileName = otherFile.FileName,
            FileId = otherFile.FileId,
            BlobName = otherFile.BlobName
        };
    }

    public RegistrationSubmissionStatus Status { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime? StatusPendingDate { get; set; }

    public DateTime TimeAndDateOfSubmission { get; set; }
    public bool SubmittedOnTime { get; set; }
    public string SubmittedBy { get; set; }
    public ServiceRole? AccountRole { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string DeclaredBy { get; set; }

    // Files with download links
    public List<FileDetails> Files { get; set; }
    public int? ServiceRoleId { get; private set; }

    public SubmissionDetailsViewModel()
    {
        Files = [];
    }

    public string DisplayAppropriateSubmissionDate()
    {
        const string format = "dd MMMM yyyy HH:mm:ss";

        return Status switch
        {
            RegistrationSubmissionStatus.Granted or RegistrationSubmissionStatus.Refused or RegistrationSubmissionStatus.Queried => StatusPendingDate?.ToString(format, CultureInfo.InvariantCulture),
            RegistrationSubmissionStatus.Cancelled => DecisionDate?.ToString(format, CultureInfo.InvariantCulture),
            _ => TimeAndDateOfSubmission.ToString(format, CultureInfo.InvariantCulture),
        } ?? TimeAndDateOfSubmission.ToString(format, CultureInfo.InvariantCulture);
    }

    public static implicit operator RegistrationSubmissionOrganisationSubmissionSummaryDetails(SubmissionDetailsViewModel details) => details is null  ? null : new()
    {
        AccountRoleId = details.ServiceRoleId,
        Telephone = details.Telephone,
        Email = details.Email,
        DeclaredBy = details.DeclaredBy,
        SubmittedBy = details.SubmittedBy,
        DecisionDate = details.DecisionDate,
        Status = details.Status,
        SubmittedOnTime = details.SubmittedOnTime,
        TimeAndDateOfSubmission = details.TimeAndDateOfSubmission,
        Files = details.Files.Select(file => (RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails)file).ToList()
    };

    public static implicit operator SubmissionDetailsViewModel(RegistrationSubmissionOrganisationSubmissionSummaryDetails details) {
        if (details is null)
        {
            return null;
        }

        return new()
        {
            ServiceRoleId = details.AccountRoleId,
            AccountRole = details.AccountRoleId.HasValue ? (ServiceRole)details.AccountRoleId : null,
            Telephone = details.Telephone,
            Email = details.Email,
            DeclaredBy = details.DeclaredBy,
            SubmittedBy = details.SubmittedBy,
            DecisionDate = details.DecisionDate,
            Status = details.Status,
            SubmittedOnTime = details.SubmittedOnTime,
            TimeAndDateOfSubmission = details.TimeAndDateOfSubmission,
            Files = details.Files.Select(file => (SubmissionDetailsViewModel.FileDetails)file).ToList()
        };
    }
}
