using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class ManageRegistrationMaterialViewModel
{
    public int Id { get; init; }

    public required string MaterialName { get; init; }

    public ApplicationStatus? Status { get; init; }

    public RegistrationTaskViewModel? MaterialWasteLicensesTask { get; init; }

    public RegistrationTaskViewModel? InputsAndOutputsTask { get; init; }

    public RegistrationTaskViewModel? SamplingAndInspectionPlanTask { get; init; }

    public RegistrationTaskViewModel? MaterialDetailsTask { get; init; }

    public RegistrationTaskViewModel? OverseasReprocessorTask { get; init; }
}