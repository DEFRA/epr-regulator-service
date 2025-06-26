namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces
{
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions;

    public interface IManageRegistrationSubmissionsService
    {
        public Task<RegistrationSubmissionsDto> GetRegistrationSubmissionsAsync(int? pageNumber);

        public Task<RegistrationSubmissionDetailsDto> GetRegistrationSubmissionDetailsAsync(Guid submissionId);
    }
}
