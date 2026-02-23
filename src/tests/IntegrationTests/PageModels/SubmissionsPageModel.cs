namespace IntegrationTests.PageModels;

public class SubmissionsPageModel : PageModelBase, IPageModelFactory<SubmissionsPageModel>
{
    public SubmissionsPageModel(string html) : base(html)
    {
    }

    public static SubmissionsPageModel FromContent(string html) => new(html);

    public string? AgencyNameCaption => _document
        .QuerySelector("h1.govuk-caption-xl")
        ?.TextContent
        .Trim();

    public string? PageHeading => _document
        .QuerySelector("h2.govuk-heading-xl.govuk-\\!-margin-bottom-8")
        ?.TextContent
        .Trim();
}
