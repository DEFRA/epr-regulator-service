using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    using System.Globalization;

    using Core.Models;

    [AttributeUsage(AttributeTargets.Property)]
    public class TransferNationDetailsAttribute : ValidationAttribute
    {
        private readonly string _selectedNationIdPropertyName;
        private readonly string _requiredErrorMessage;
        private readonly string _lengthExceededErrorMessage;
        private readonly int _maxLength;

        public TransferNationDetailsAttribute(
            string selectedNationIdPropertyName, 
            string requiredErrorMessage, 
            string lengthExceededErrorMessage, 
            int maxLength)
        {
            _selectedNationIdPropertyName = selectedNationIdPropertyName;
            _requiredErrorMessage = requiredErrorMessage;
            _lengthExceededErrorMessage = lengthExceededErrorMessage;
            _maxLength = maxLength;
        }
    
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var agencyIndex = (int?)validationContext
                .ObjectInstance.GetType()
                .GetProperty(_selectedNationIdPropertyName)
                ?.GetValue(validationContext.ObjectInstance);
            var transferNotes = value as List<TransferNote>;
            
            return agencyIndex == null ?
                // no agency selected, so no need to return an error for the transfer details
                ValidationResult.Success : Validate(transferNotes, agencyIndex);
        }

        private ValidationResult Validate(IEnumerable<TransferNote> transferNotes, int? agencyIndex)
        {
            foreach (var transferNote in transferNotes)
            {
                if (transferNote.AgencyIndex != agencyIndex)
                {
                    continue;
                }

                var errorLink = agencyIndex.Value.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) + "-notes";
                if (string.IsNullOrEmpty(transferNote.Notes))
                {
                    return new ValidationResult(_requiredErrorMessage, new []{ errorLink });
                }

                if (transferNote.Notes.Length > _maxLength)
                {
                    return new ValidationResult(_lengthExceededErrorMessage, new []{ errorLink});
                }
            }

            return ValidationResult.Success;
        }
    }
}