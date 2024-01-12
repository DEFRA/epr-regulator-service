namespace EPR.RegulatorService.Frontend.Web.ViewModels.InviteNewApprovedPerson;

using System.ComponentModel.DataAnnotations;

public class EnterPersonEmailModel : IValidatableObject
{
    private const int MaximumSearchLength = 254;

    private static readonly string[] _emailInvalidErrors = {
        "PersonEmail.InvalidFormat"
    };

    private static readonly string[] _emailTooLongErrors = {
        "PersonEmail.TooLongError"
    };
    
    [Required(ErrorMessage = "PersonEmail.MissingError")]
    public string Email { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult> { };

        if (!Email.Contains('.') && !Email.Contains('@'))
        {
            validationResults.AddRange(_emailInvalidErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(Email)})));
        }
        if (Email.Length > MaximumSearchLength)
        {
            validationResults.AddRange(_emailTooLongErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(Email)})));
        }
        return validationResults;
    }
}