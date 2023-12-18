using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Core.Models.Pagination;
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

    Task<PaginatedList<Submission>> GetOrganisationSubmissions(
        string? organisationName,
        string? organisationReference,
        string? organisationType,
        string[]? status,
        int currentPage = 1);

    Task<EndpointResponseStatus> SubmitPoMDecision(RegulatorPoMDecisionCreateRequest request);

    Task<PaginatedList<OrganisationSearchResult>> GetOrganisationBySearchTerm(string searchTerm, int currentPage = 1);

    Task<List<OrganisationUser>> GetProducerOrganisationUsersByOrganisationExternalId(Guid externalId);

}
