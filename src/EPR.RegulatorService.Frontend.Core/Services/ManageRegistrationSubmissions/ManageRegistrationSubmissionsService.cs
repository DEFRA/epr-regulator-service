namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions
{
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.DTOs;
    using EPR.RegulatorService.Frontend.Core.Http.ManageRegistrationSubmissions.Interfaces;
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;

    public class ManageRegistrationSubmissionsService(
        IManageRegistrationSubmissionsApiClient apiClient) : IManageRegistrationSubmissionsService
    {
        public Task<RegistrationSubmissionsDto> GetRegistrationSubmissionsAsync(int? pageNumber) => throw new NotImplementedException();

        public async Task<RegistrationSubmissionDetailsDto> GetRegistrationSubmissionDetailsAsync(Guid submissionId) =>
            await apiClient.GetRegistrationSubmissionDetailsAsync(submissionId);
    }
}
