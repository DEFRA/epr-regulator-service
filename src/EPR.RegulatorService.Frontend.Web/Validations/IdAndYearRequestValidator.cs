using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

using FluentValidation;

namespace EPR.RegulatorService.Frontend.Web.Validations;

public class IdAndYearRequestValidator : AbstractValidator<IdAndYearRequest>
{
    public IdAndYearRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2099) // or change 2099 to a more dynamic upper bound if needed
            .WithMessage("Year must be between 2000 and 2099.");
    }
}