namespace EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter
{
    using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;

    public interface IRegistrationService
    {
        RegistrationDto? GetRegistrationById(int id);
    }
}