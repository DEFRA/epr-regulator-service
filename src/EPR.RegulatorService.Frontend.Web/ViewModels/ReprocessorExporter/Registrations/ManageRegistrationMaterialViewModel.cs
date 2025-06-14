namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using Core.Enums.ReprocessorExporter;

public class ManageRegistrationMaterialViewModel
{
    public Guid Id { get; init; }

    public required string MaterialName { get; init; }

    public DateTime? DeterminationDate { get; set; }

    public ApplicationStatus? Status { get; init; }

    public required string StatusCssClass { get; init; }

    public required string StatusText { get; init; }

    public string? StatusUpdatedBy { get; init; }

    public DateTime? StatusUpdatedDate { get; init; }

    public string? RegistrationReferenceNumber { get; init; }

    public string? ApplicationReferenceNumber { get; init; }

    public RegistrationTaskViewModel? MaterialWasteLicensesTask { get; init; }

    public RegistrationTaskViewModel? InputsAndOutputsTask { get; init; }

    public RegistrationTaskViewModel? SamplingAndInspectionPlanTask { get; init; }

    public RegistrationTaskViewModel? MaterialDetailsTask { get; init; }

    public RegistrationTaskViewModel? OverseasReprocessorTask { get; init; }

    public RegistrationTaskViewModel? RegistrationStatusTask { get; init; }
}