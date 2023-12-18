using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CharacterCountAttribute : ValidationAttribute
    {
        private readonly string _requiredErrorMessage;
        private readonly string _lengthExceededErrorMessage;
        private readonly int _maxLength;

        public CharacterCountAttribute(
            string requiredErrorMessage, 
            string lengthExceededErrorMessage, 
            int maxLength)
        {
            _requiredErrorMessage = requiredErrorMessage;
            _lengthExceededErrorMessage = lengthExceededErrorMessage;
            _maxLength = maxLength;
        }
    
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string? text = value.ToString();
                if (text != null && text.Length > _maxLength)
                {
                    return new ValidationResult(_lengthExceededErrorMessage);
                }
                
                return ValidationResult.Success;
            }
            
            return new ValidationResult(_requiredErrorMessage);
        }
    }
}