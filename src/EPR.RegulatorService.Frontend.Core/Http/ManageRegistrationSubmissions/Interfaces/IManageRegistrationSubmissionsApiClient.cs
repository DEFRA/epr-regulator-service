namespace EPR.RegulatorService.Frontend.Core.Http.ManageRegistrationSubmissions.Interfaces
{
    using System;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions;

    public interface IManageRegistrationSubmissionsApiClient
    {
        Task<RegistrationSubmissionDetailsDto> GetRegistrationSubmissionDetailsAsync(Guid submissionId);
    }
}
