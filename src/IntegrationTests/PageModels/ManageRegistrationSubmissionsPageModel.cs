namespace IntegrationTests.PageModels;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

/// <summary>
/// Page Model for the /regulators/manage-registration-submissions page.
/// Provides an abstraction over AngleSharp for better test readability and maintainability.
/// </summary>
public class ManageRegistrationSubmissionsPageModel
{
    private readonly IHtmlDocument _document;

    public ManageRegistrationSubmissionsPageModel(string html)
    {
        var parser = new HtmlParser();
        _document = parser.ParseDocument(html);
    }

    public string? AgencyNameCaption => _document
        .QuerySelector("h1.govuk-caption-xl")
        ?.TextContent
        .Trim();

    public string? PageHeading => _document
        .QuerySelector("h2.govuk-heading-xl.govuk-\\!-margin-bottom-8")
        ?.TextContent
        .Trim();

    public IEnumerable<RegistrationSubmissionRow> GetTableRows()
    {
        var rows = _document.QuerySelectorAll("table.govuk-table tbody tr.govuk-table__row");
        return rows.Select(row =>
        {
            var cells = row.QuerySelectorAll("td");
            return new RegistrationSubmissionRow
            {
                OrganisationName = cells[0].QuerySelector("a")?.TextContent.Trim(),
                OrganisationType = cells[0].QuerySelector("span.orgNameFilter")?.TextContent.Trim(),
                OrganisationReference = ExtractCellValue(cells[1]),
                ApplicationDate = ExtractCellValue(cells[2]),
                Year = ExtractCellValue(cells[3]),
                Status = cells[4].QuerySelector("govuk-tag")?.GetAttribute("content")
            };
        }).ToList();
    }

    private static string? ExtractCellValue(AngleSharp.Dom.IElement cell)
    {
        // Clone the cell to avoid modifying the original document
        var cellClone = (AngleSharp.Dom.IElement)cell.Clone(true);

        // Remove the responsive-table__heading span and extract the actual value
        var heading = cellClone.QuerySelector("span.responsive-table__heading");
        if (heading != null)
        {
            heading.Remove();
        }
        var parts = cellClone.TextContent
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
        return parts.Length > 0 ? parts[0] : null;
    }
}

public class RegistrationSubmissionRow
{
    public string? OrganisationName { get; set; }
    public string? OrganisationType { get; set; }
    public string? OrganisationReference { get; set; }
    public string? ApplicationDate { get; set; }
    public string? Year { get; set; }
    public string? Status { get; set; }
}
