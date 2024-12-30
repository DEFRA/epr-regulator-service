namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

public abstract class BaseSubmissionDetailsViewModel
{
    public bool IsRegistrationSubmission { get; set; }

    public string ApplicationReferenceNumber { get; set; }

    public Guid SubmissionId { get; set; }

    public string NationCode { get; set; }

    public DateTime RegistrationDateTime { get; set; }

    public List<CsoMembershipDetailsDto> CSOMembershipDetails { get; set; }

    public ProducerDetailsDto ProducerDetails { get; set; }
}