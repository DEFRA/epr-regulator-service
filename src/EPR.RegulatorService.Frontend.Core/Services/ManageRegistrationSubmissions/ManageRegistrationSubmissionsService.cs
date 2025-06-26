namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions
{
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Exceptions.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Http.ManageRegistrationSubmissions.Interfaces;
    using EPR.RegulatorService.Frontend.Core.Logging.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;

    using Microsoft.Extensions.Logging;

    public class ManageRegistrationSubmissionsService(
        IManageRegistrationSubmissionsApiClient apiClient,
        ILogger<ManageRegistrationSubmissionsService> logger) : IManageRegistrationSubmissionsService
    {
        public Task<RegistrationSubmissionsDto> GetRegistrationSubmissionsAsync(int? pageNumber) => throw new NotImplementedException();

        public async Task<RegistrationSubmissionDetailsDto> GetRegistrationSubmissionDetailsAsync(Guid submissionId)
        {
            try
            {
                return await apiClient.GetRegistrationSubmissionDetailsAsync(submissionId);
            }
            catch (RegistrationSubmissionNotFoundException)
            {
                logger.RegistrationSubmissionNotFound(submissionId);
                throw;
            }
            catch (Exception ex)
            {
                logger.GetRegistrationSubmissionDetailsFailed(submissionId, ex);
                throw;
            }
        }
    }
}
