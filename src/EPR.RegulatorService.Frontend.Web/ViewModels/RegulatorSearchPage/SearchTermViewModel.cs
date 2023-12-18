using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorSearchPage;
public class SearchTermViewModel : IValidatableObject
{
    private const int MinimumSearchLength = 3;
    private const int MaximumSearchLength = 254;

    private static readonly string[] _searchTermTooShortErrors = {
        "SearchTerm.OrganisationName.TooShortError",
        "SearchTerm.OrganisationId.TooShortError"
    };

    private static readonly string[] _searchTermTooLongErrors = {
        "SearchTerm.OrganisationName.TooLongError",
        "SearchTerm.OrganisationId.TooLongError"
    };

    [Required(ErrorMessage = "SearchTerm.MissingError")]
    public string SearchTerm { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var validationResults = new List<ValidationResult> { };

        if (SearchTerm.Length < MinimumSearchLength)
        {
            validationResults.AddRange(_searchTermTooShortErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(SearchTerm)})));
        }
        else if (SearchTerm.Length > MaximumSearchLength)
        {
            validationResults.AddRange(_searchTermTooLongErrors.Select(
                errorString => new ValidationResult(errorString, new[] {nameof(SearchTerm)})));
        }
        return validationResults;
    }
}