namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public class RegistrationSubmissionDetailsViewModel : BaseSubmissionDetailsViewModel
{
    public Guid OrganisationId { get; set; }

    public string OrganisationReference { get; set; }

    public string OrganisationName { get; set; }

    public string? RegistrationReferenceNumber { get; set; }

    public RegistrationSubmissionOrganisationType OrganisationType { get; set; }

    public BusinessAddress BusinessAddress { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public string RegisteredNation { get; set; }

    public int NationId { get; set; }

    public string PowerBiLogin { get; set; }

    public RegistrationSubmissionStatus Status { get; set; }

    public SubmissionDetailsViewModel SubmissionDetails { get; set; }

    public int RegistrationYear { get; set; }

    public string? ProducerComments { get; set; }

    public string? RegulatorComments { get; set; }

    public string? RejectReason { get; set; } = string.Empty;

    public string? CancellationReason { get; set; } = string.Empty;

    public List<CsoMembershipDetailsDto> CSOMembershipDetails { get; set; }

    public ProducerDetailsDto ProducerDetails { get; set; }

    public bool IsResubmission { get; set; }

    public RegistrationSubmissionStatus? ResubmissionStatus { get; set; }
    public string ResubmissionFileId { get; set; }

    
}