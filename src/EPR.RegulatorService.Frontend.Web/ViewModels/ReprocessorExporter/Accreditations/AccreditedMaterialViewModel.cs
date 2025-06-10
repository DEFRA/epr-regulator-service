namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Helpers;

public class AccreditedMaterialViewModel
{
    public Guid Id { get; set; }

    public RegistrationTaskViewModel RegistrationStatusTask { get; init; }

    public string MaterialName { get; set; } = string.Empty;

    // Although the backend model supports multiple accreditations per material,
    // the ManageAccreditations view is designed to display exactly one accreditation per material for a selected year.
    // Therefore, the ViewModel keeps a single `Accreditation` property.
    // If multiple exist for the same year, it indicates a backend data issue and an error is thrown in the service layer.
    public AccreditationDetailsViewModel? Accreditation { get; set; } = new();

    public ApplicationStatus? RegistrationStatusRaw { get; init; }

    public bool ShouldDisplay =>
        AccreditationDisplayHelper.ShouldDisplayMaterial(RegistrationStatusRaw, Accreditation);
}