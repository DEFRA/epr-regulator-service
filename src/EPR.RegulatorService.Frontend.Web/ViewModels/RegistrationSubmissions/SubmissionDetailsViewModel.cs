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
    }

    public RegistrationSubmissionStatus Status { get; set; }
    public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }
    public DateTime? LatestDecisionDate { get; set; }
    public DateTime? ResubmissionDecisionDate { get; set; }
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

    public string ResubmissionFileId { get; set; }

    public SubmissionDetailsViewModel()
    {
        Files = [];
    }

    public string DisplayAppropriateSubmissionDate()
    {
        const string format = "dd MMMM yyyy HH:mm:ss";

        var targetDate = Status switch
        {
            RegistrationSubmissionStatus.Granted => RegistrationDate,
            RegistrationSubmissionStatus.Refused or
            RegistrationSubmissionStatus.Queried or
            RegistrationSubmissionStatus.Updated or
            RegistrationSubmissionStatus.Accepted or
            RegistrationSubmissionStatus.Rejected => LatestDecisionDate,
            RegistrationSubmissionStatus.Cancelled => StatusPendingDate ?? LatestDecisionDate,
            _ => TimeAndDateOfSubmission,
        } ?? TimeAndDateOfSubmission;

        return targetDate.ToString(Status switch { RegistrationSubmissionStatus.Cancelled when StatusPendingDate.HasValue => "dd MMMM yyyy", _ => format }, CultureInfo.InvariantCulture);
    }

    private static ServiceRole? GetAccountRole(int? serviceRoleId)
    {
        ServiceRole? retVal = serviceRoleId.HasValue ? (ServiceRole)serviceRoleId : null;

        return retVal;
    }
}
