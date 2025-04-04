namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

public class RegistrationTaskViewModel
{
    public int Id { get; init; }

    public required string StatusCssClass { get; init; }

    public required string StatusText { get; init; }
}