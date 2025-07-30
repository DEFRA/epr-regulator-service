using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
using EPR.RegulatorService.Frontend.Core.Models.Registrations;
using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Core.Models.Submissions;

namespace EPR.RegulatorService.Frontend.Core.Services;

public interface IFacadeService
{
    Task<string> GetTestMessageAsync();

    Task<PaginatedList<OrganisationApplications>> GetUserApplicationsByOrganisation(
        string? applicationType,
        string? organisationName,
        int currentPage = 1);

    Task<OrganisationEnrolments> GetOrganisationEnrolments(Guid organisationId);

    Task<RegulatorCompanyDetailsModel> GetRegulatorCompanyDetails(Guid organisationId);

    Task<EndpointResponseStatus> TransferOrganisationNation(OrganisationTransferNationRequest organisationNationTransfer);

    Task<EndpointResponseStatus> UpdateEnrolment(UpdateEnrolment updateEnrolment);

    Task<EndpointResponseStatus> SendEnrolmentEmails(EnrolmentDecisionRequest request);

    Task<EndpointResponseStatus> EnrolInvitedUser(EnrolInvitedUserRequest request);

    Task<HttpResponseMessage> GetUserAccountDetails();

    Task<EndpointResponseStatus> SubmitPoMDecision(RegulatorPoMDecisionCreateRequest request);

    Task<PaginatedList<OrganisationSearchResult>> GetOrganisationBySearchTerm(string searchTerm, int currentPage = 1);

    Task<List<OrganisationUser>> GetProducerOrganisationUsersByOrganisationExternalId(Guid externalId);

    Task<EndpointResponseStatus> RemoveApprovedUser(RemoveApprovedUserRequest request);

    Task<PaginatedList<T>> GetOrganisationSubmissions<T>(
        string? organisationName,
        string? organisationReference,
        OrganisationType? organisationType,
        string[]? status,
        int[]? submissionYears,
        string[]? submissionPeriods,
        int currentPage = 1) where T : AbstractSubmission;

    Task<Stream> GetRegistrationSubmissionsCsv(GetRegistrationSubmissionsCsvRequest request);

    Task<Stream> GetPackagingSubmissionsCsv(GetPackagingSubmissionsCsvRequest request);

    Task<EndpointResponseStatus> AddRemoveApprovedUser(AddRemoveApprovedUserRequest request);

    Task<EndpointResponseStatus> SubmitRegistrationDecision(RegulatorRegistrationDecisionCreateRequest request);
    Task<HttpResponseMessage> GetFileDownload(FileDownloadRequest request);

    Task<PaginatedList<RegistrationSubmissionOrganisationDetails>> GetRegistrationSubmissions(RegistrationSubmissionsFilterModel filters);

    Task<RegistrationSubmissionOrganisationDetails> GetRegistrationSubmissionDetails(Guid submissionId, RegistrationSubmissionOrganisationType organisationType);

    Task<EndpointResponseStatus> SubmitRegulatorRegistrationDecisionAsync(RegulatorDecisionRequest request);

    Task SubmitRegistrationFeePaymentAsync(FeePaymentRequest request);

    Task SubmitPackagingDataResubmissionFeePaymentEventAsync(FeePaymentRequest request);

    Task<PomPayCalParametersResponse> GetPomPayCalParameters(Guid submissionId, Guid? complianceSchemeId);
}
