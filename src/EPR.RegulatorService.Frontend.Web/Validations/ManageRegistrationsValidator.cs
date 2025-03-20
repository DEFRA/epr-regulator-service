using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

namespace EPR.RegulatorService.Frontend.Web.Validations;

public class ManageRegistrationsValidator : AbstractValidator<ManageRegistrationsRequest>
{
    public ManageRegistrationsValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0.");
    }
}