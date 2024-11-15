using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateValidationAttribute(string invalidDateErrorMessage,
                                            string includeADayErrorMessage,
                                            string includeAMonthErrorMessage,
                                            string includeAYearErrorMessage,
                                            string enterACancellationDate,
                                            string mustBeARealDateErrorMessage,
                                            string pastDateErrorMessage) : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var viewModel = (CancelDateRegistrationSubmissionViewModel)validationContext.ObjectInstance;

            if (AllFieldsAreEmpty(viewModel))
            {
                return new ValidationResult(enterACancellationDate, ["CancellationDate"]);
            }

            var result = IsValidDayField(viewModel.Day);

            if (!result.isValid)
            {
                return result.validationResult;
            }

            result = IsValidMonthField(viewModel.Month);

            if (!result.isValid)
            {
                return result.validationResult;
            }

            result = IsValidYearField(viewModel.Year);

            if (!result.isValid)
            {
                return result.validationResult;
            }

            result = IsInvalidDate((int)viewModel.Day, (int)viewModel.Month, (int)viewModel.Year);

            if (!result.isValid)
            {
                return result.validationResult;
            }

            var (isValid, cancellationDate, validationResult) = IsDateValid((int)viewModel.Day, (int)viewModel.Month, (int)viewModel.Year);

            if (!isValid)
            {
                return validationResult;
            }

            viewModel.CancellationDate = cancellationDate;

            result = PastDateValidation(cancellationDate);

            return !result.isValid ? result.validationResult : ValidationResult.Success;
        }


        private static bool AllFieldsAreEmpty(CancelDateRegistrationSubmissionViewModel viewModel) => viewModel.Day == null && viewModel.Month == null && viewModel.Year == null;

        private (bool isValid, ValidationResult validationResult) IsValidDayField(int? day)
        {
            bool result;
            if (day == null)
            {
                result = false;
                return (result, new ValidationResult(includeADayErrorMessage, ["CancellationDate"]));
            }
            result = true;
            return (result, ValidationResult.Success);
        }

        private (bool isValid, ValidationResult validationResult) IsValidMonthField(int? month)
        {
            bool result;
            if (month == null)
            {
                result = false;
                return (result, new ValidationResult(includeAMonthErrorMessage, ["CancellationDate"]));
            }
            result = true;
            return (result, ValidationResult.Success);
        }


        private (bool isValid, ValidationResult validationResult) IsValidYearField(int? year)
        {
            bool result;
            if (year == null)
            {
                result = false;
                return (result, new ValidationResult(includeAYearErrorMessage, ["CancellationDate"]));
            }
            result = true;
            return (result, ValidationResult.Success);
        }

        private (bool isValid, ValidationResult validationResult) PastDateValidation(DateTime cancellationDate)
        {
            bool result;
            var validCancellationDate = new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Local);

            if (cancellationDate <= validCancellationDate)
            {
                result = false;
                return (result, new ValidationResult(pastDateErrorMessage, ["CancellationDate"]));
            }
            result = true;
            return (result, ValidationResult.Success);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private (bool isValid, DateTime cancellationDate, ValidationResult validationResult) IsDateValid(int? day, int? month, int? year)
        {
            bool result;
            string dateString = $"{day}/{month}/{year}";

            string[] formats = ["dd/MM/yyyy", "d/M/yyyy"];

            bool isDateValid = DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
            if (!isDateValid)
            {
                result = false;
                return (result, date ,new ValidationResult(mustBeARealDateErrorMessage, ["CancellationDate"]));
            }

            result = true;
            return (result, date, ValidationResult.Success);
        }

        private (bool isValid, ValidationResult validationResult) IsInvalidDate(int day, int month, int year)
        {
            bool result;
            if (day is not (>= 1 and <= 31))
            {
                result = false;
                return (result, new ValidationResult(invalidDateErrorMessage, ["CancellationDate"]));
            }

            if (month is not (>= 1 and <= 12))
            {
                result = false;
                return (result, new ValidationResult(invalidDateErrorMessage, ["CancellationDate"]));
            }

            if (year is not (>= 2024 and <= 9999))
            {
                result = false;
                return (result, new ValidationResult(invalidDateErrorMessage, ["CancellationDate"]));
            }
            result = true;
            return (result, ValidationResult.Success);
        }
    }
}