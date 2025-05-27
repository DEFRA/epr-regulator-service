using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

using FluentValidation;

namespace EPR.RegulatorService.Frontend.Web.Validations;

public class IdRequestValidator : AbstractValidator<IdRequest>
{
    public IdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage("ID is required.");
    }
}