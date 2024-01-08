namespace EPR.RegulatorService.Frontend.Core.Models.Registrations;

public class RegistrationFiltersModel
{
    public string? SearchOrganisationName { get; set; } = string.Empty;
    public string? SearchOrganisationId { get; set; } = string.Empty;
    public bool IsDirectProducerChecked { get; set; }
    public bool IsComplianceSchemeChecked { get; set; }
    public bool IsPendingRegistrationChecked { get; set; }
    public bool IsAcceptedRegistrationChecked { get; set; }
    public bool IsRejectedRegistrationChecked { get; set; }
    public bool ClearFilters { get; set; }
    public bool IsFilteredSearch { get; set; }
}
