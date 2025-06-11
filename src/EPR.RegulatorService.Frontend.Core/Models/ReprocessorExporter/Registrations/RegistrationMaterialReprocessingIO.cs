using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;

public class RegistrationMaterialReprocessingIO
{
    public Guid RegistrationId { get; set; }
    public required string OrganisationName { get; init; }
    public required string SiteAddress { get; init; }
    public Guid RegistrationMaterialId { get; set; }
    public required string MaterialName { get; set; }
    public required string SourcesOfPackagingWaste { get; set; }
    public required string PlantEquipmentUsed { get; set; }
    public bool ReprocessingPackagingWasteLastYearFlag { get; set; }
    public decimal UKPackagingWasteTonne { get; set; }
    public decimal NonUKPackagingWasteTonne { get; set; }
    public decimal NotPackingWasteTonne { get; set; }
    public decimal SenttoOtherSiteTonne { get; set; }
    public decimal ContaminantsTonne { get; set; }
    public decimal ProcessLossTonne { get; set; }
    public decimal TotalInputs { get; set; }
    public decimal TotalOutputs { get; set; }
    public Guid? RegulatorApplicationTaskStatusId { get; init; }
    public RegulatorTaskStatus TaskStatus { get; init; }
}