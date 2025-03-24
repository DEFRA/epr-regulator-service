using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;

public interface IRegistrationService
{
    Task<Registration> GetRegistrationByIdAsync(int id);
}