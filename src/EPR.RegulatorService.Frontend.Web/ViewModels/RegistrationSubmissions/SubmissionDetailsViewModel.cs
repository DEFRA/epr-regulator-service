namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Globalization;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.Constants;


using static EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.RegistrationSubmissionOrganisationSubmissionSummaryDetails;

using ServiceRole = Core.Enums.ServiceRole;

public class SubmissionDetailsViewModel
{
    public class FileDetails
    {
        public FileType Type { get; set; }
        public string DownloadType { get; set; }

        public string Label { get; set; }
        public string FileName { get; set; }
        public Guid? FileId { get; set; }
        public string? BlobName { get; set; }

        public static implicit operator FileDetails(RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails otherFile) => otherFile is null ? null : new FileDetails
        {
            Type = otherFile.Type,
            DownloadType = otherFile.Type switch
            {
                FileType.company => FileDownloadTypes.OrganisationDetails,
                FileType.partnership => FileDownloadTypes.PartnershipDetails,
                FileType.brands => FileDownloadTypes.BrandDetails,
                _ => ""
            },
            Label = otherFile.Type switch
            {
                FileType.company => "SubmissionDetails.OrganisationDetails",
                FileType.partnership => "SubmissionDetails.PartnerDetails",
                FileType.brands => "SubmissionDetails.BrandDetails",
                _ => ""
            },
            FileName = otherFile.FileName,
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
    public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime? StatusPendingDate { get; set; }

    public DateTime TimeAndDateOfSubmission { get; set; }
    public DateTime? TimeAndDateOfResubmission { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public bool SubmittedOnTime { get; set; }
    public string SubmittedBy { get; set; }
    public ServiceRole? AccountRole { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string DeclaredBy { get; set; }

    // Files with download links
    public List<FileDetails> Files { get; set; }
    public int? AccountRoleId { get; set; }

    public bool IsResubmission { get; set; }

    public SubmissionDetailsViewModel()
    {
        Files = [];
    }

    public string DisplayAppropriateSubmissionDate()
    {
        const string format = "dd MMMM yyyy HH:mm:ss";

        var targetDate = Status switch
        {
            RegistrationSubmissionStatus.Granted or RegistrationSubmissionStatus.Refused or RegistrationSubmissionStatus.Queried or RegistrationSubmissionStatus.Updated => DecisionDate,
            RegistrationSubmissionStatus.Cancelled => StatusPendingDate ?? DecisionDate,
            _ => TimeAndDateOfSubmission,
        } ?? TimeAndDateOfSubmission;

        return targetDate.ToString(Status switch { RegistrationSubmissionStatus.Cancelled => "dd MMMM yyyy", _ => format }, CultureInfo.InvariantCulture);
    }

    public static implicit operator RegistrationSubmissionOrganisationSubmissionSummaryDetails(SubmissionDetailsViewModel details)
    {
        if (details is null)
        {
            return null;
        }

        var result = new RegistrationSubmissionOrganisationSubmissionSummaryDetails
        {
            AccountRoleId = details.AccountRoleId,
            Telephone = details.Telephone,
            Email = details.Email,
            DeclaredBy = details.DeclaredBy,
            SubmittedBy = details.SubmittedBy,
            DecisionDate = details.DecisionDate,
            Status = details.Status,
            ResubmissionStatus = details.ResubmissionStatus,
            SubmittedOnTime = details.SubmittedOnTime,
            TimeAndDateOfSubmission = details.TimeAndDateOfSubmission,
            TimeAndDateOfResubmission = details.TimeAndDateOfResubmission,
            RegistrationDate = details.RegistrationDate,
            IsResubmission = details.IsResubmission
        };

        if (details.Files != null)
        {
            result.Files = details.Files
                .Select(file => (RegistrationSubmissionOrganisationSubmissionSummaryDetails.FileDetails)file)
                .ToList();
        }

        return result;
    }


    public static implicit operator SubmissionDetailsViewModel(RegistrationSubmissionOrganisationSubmissionSummaryDetails details)
    {
        if (details is null)
        {
            return null;
        }

        return new()
        {
            AccountRoleId = details.AccountRoleId,
            AccountRole = GetAccountRole(details.AccountRoleId),
            Telephone = details.Telephone,
            Email = details.Email,
            DeclaredBy = details.DeclaredBy,
            SubmittedBy = details.SubmittedBy,
            DecisionDate = details.DecisionDate,
            Status = details.Status,
            ResubmissionStatus = details.ResubmissionStatus ?? RegistrationSubmissionStatus.Pending,
            SubmittedOnTime = details.SubmittedOnTime,
            TimeAndDateOfSubmission = details.TimeAndDateOfSubmission,
            TimeAndDateOfResubmission = details.TimeAndDateOfResubmission ?? details.TimeAndDateOfSubmission,
            RegistrationDate = details.RegistrationDate,
            Files = details.Files.Select(file => (SubmissionDetailsViewModel.FileDetails)file).ToList(),
            IsResubmission = details.IsResubmission
        };
    }

    private static ServiceRole? GetAccountRole(int? serviceRoleId)
    {
        ServiceRole? retVal = serviceRoleId.HasValue ? (ServiceRole)serviceRoleId : null;

        return retVal;
    }
}
