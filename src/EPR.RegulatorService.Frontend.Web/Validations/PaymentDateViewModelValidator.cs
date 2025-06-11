using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus; 

using FluentValidation;

namespace EPR.RegulatorService.Frontend.Web.Validations;

public class PaymentDateViewModelValidator : AbstractValidator<PaymentDateViewModel>
{
    public PaymentDateViewModelValidator()
    {
        RuleFor(x => x.Day)
            .NotNull().WithMessage("Enter date in DD MM YYYY format")
            .InclusiveBetween(1, 31).WithMessage("Enter a valid day");

        RuleFor(x => x.Month)
            .NotNull().WithMessage("Enter date in DD MM YYYY format")
            .InclusiveBetween(1, 12).WithMessage("Enter a valid month");

        RuleFor(x => x.Year)
            .NotNull().WithMessage("Enter a valid year in YYYY format")
            .InclusiveBetween(2000, 2100).WithMessage("Enter a valid year in YYYY format");

        RuleFor(x => x)
            .Must(HasValidDate).WithMessage("Enter a valid date")
            .When(x => x.Day.HasValue && x.Month.HasValue && x.Year.HasValue);

        RuleFor(x => x)
            .Must(BeTodayOrPast).WithMessage("Date must be either today or in the past")
            .When(x => x.Day.HasValue && x.Month.HasValue && x.Year.HasValue && HasValidDate(x));
    }

    private bool HasValidDate(PaymentDateViewModel model)
    {
        return DateOnly.TryParse(
            $"{model.Year:D4}-{model.Month:D2}-{model.Day:D2}", out _);
    }

    private bool BeTodayOrPast(PaymentDateViewModel model)
    {
        var parsed = DateOnly.Parse($"{model.Year:D4}-{model.Month:D2}-{model.Day:D2}");
        return parsed <= DateOnly.FromDateTime(DateTime.Today);
    }
}