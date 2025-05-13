namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class MaterialWasteLicencesViewModel
{
    public int RegistrationMaterialId { get; set; }

    public required string MaterialName { get; set; }

    public required string PermitType { get; set; }

    public required string ReferenceNumberLabel { get; set; }

    public required string[] LicenceNumbers { get; set; }

    public decimal? CapacityTonne { get; set; }

    public string? CapacityPeriod { get; set; }

    public decimal MaximumReprocessingCapacityTonne { get; set; }

    public required string MaximumReprocessingPeriod { get; set; }
}