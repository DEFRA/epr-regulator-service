namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public abstract class BaseSubmissionDetailsViewModel
{
    public string ReferenceNumber { get; set; }

    public bool ReferenceNotAvailable { get; set; }

    public bool ReferenceFieldNotAvailable { get; set; }

    public string NationCode { get; set; }

    public DateTime? RegistrationDateTime { get; set; }

    public Guid SubmissionId { get; set; }

    public int MemberCount { get; set; }
}