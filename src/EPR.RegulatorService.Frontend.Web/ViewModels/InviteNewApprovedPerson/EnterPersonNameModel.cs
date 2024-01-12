namespace EPR.RegulatorService.Frontend.Web.ViewModels.InviteNewApprovedPerson;

using System.ComponentModel.DataAnnotations;

public class EnterPersonNameModel : IValidatableObject
{
    private const int MaximumSearchLength = 34;

    private static readonly string[] _firstInvalidErrors = {
        "ErrorTitle.FirstNameDigits"
    };
    private static readonly string[] _lastInvalidErrors = {
        "ErrorTitle.LastNameDigits"
    };

    private static readonly string[] _firstTooLongErrors = {
        "ErrorTitle.FirstNameLength"
    };
    private static readonly string[] _lastTooLongErrors = {
        "ErrorTitle.LastNameLength"
    };
    
    [Required(ErrorMessage = "ErrorTitle.FirstNameNull")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "ErrorTitle.LastNameNull")]
    public string LastName { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult> { };

        if (FirstName.All(char.IsDigit) && FirstName.All(char.IsDigit))
        {
            validationResults.AddRange(_firstInvalidErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(FirstName)})));
        }
        if (LastName.All(char.IsDigit) && LastName.All(char.IsDigit))
        {
            validationResults.AddRange(_lastInvalidErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(LastName)})));
        }
        
        if (FirstName.Length > MaximumSearchLength)
        {
            validationResults.AddRange(_firstTooLongErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(FirstName)})));
        }
        if (LastName.Length > MaximumSearchLength)
        {
            validationResults.AddRange(_lastTooLongErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(LastName)})));
        }
        return validationResults;
    }
}