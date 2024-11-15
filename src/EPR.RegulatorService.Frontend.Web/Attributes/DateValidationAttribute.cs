using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateValidationAttribute(
        string invalidDateErrorMessage,
        string missingDayErrorMessage,
        string missingMonthErrorMessage,
        string missingYearErrorMessage,
        string emptyDateErrorMessage,
        string invalidRealDateErrorMessage,
        string pastDateErrorMessage) : ValidationAttribute
    {

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is not CancelDateRegistrationSubmissionViewModel viewModel)
            {
                throw new InvalidOperationException("This attribute is only valid on CancelDateRegistrationSubmissionViewModel.");
            }

            if (AreAllFieldsEmpty(viewModel))
            {
                return CreateValidationResult(emptyDateErrorMessage);
            }

            if (!ValidateField(viewModel.Day, missingDayErrorMessage, 1, 31, out var dayValidationResult))
            {
                return dayValidationResult;
            }

            if (!ValidateField(viewModel.Month, missingMonthErrorMessage, 1, 12, out var monthValidationResult))
            {
                return monthValidationResult;
            }

            if (!ValidateField(viewModel.Year, missingYearErrorMessage, 2024, 9999, out var yearValidationResult))
            {
                return yearValidationResult;
            }

            if (!TryParseDate(viewModel.Day!.Value, viewModel.Month!.Value, viewModel.Year!.Value, out var parsedDate, out var dateValidationResult))
            {
                return dateValidationResult;
            }

            viewModel.CancellationDate = parsedDate;

            return parsedDate <= new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Local) ? CreateValidationResult(pastDateErrorMessage) : ValidationResult.Success;
        }
        private static bool AreAllFieldsEmpty(CancelDateRegistrationSubmissionViewModel viewModel) =>
            !viewModel.Day.HasValue &&
            !viewModel.Month.HasValue &&
            !viewModel.Year.HasValue;

        private bool ValidateField(int? value, string errorMessage, int min, int max, out ValidationResult validationResult)
        {
            if (!value.HasValue)
            {
                validationResult = CreateValidationResult(errorMessage);
                return false;
            }

            if (value.Value < min || value.Value > max)
            {
                validationResult = CreateValidationResult(invalidDateErrorMessage);
                return false;
            }

            validationResult = ValidationResult.Success!;
            return true;
        }

        private bool TryParseDate(int day, int month, int year, out DateTime parsedDate, out ValidationResult validationResult)
        {
            string dateString = $"{day}/{month}/{year}";
            string[] formats = { "d/M/yyyy", "dd/MM/yyyy" };
            if (!DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                validationResult = CreateValidationResult(invalidRealDateErrorMessage);
                return false;
            }
            validationResult = ValidationResult.Success!;
            return true;
        }

        private static ValidationResult CreateValidationResult(string errorMessage) => new(errorMessage, ["CancellationDate"]);
    }
}