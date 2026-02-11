namespace IntegrationTests.PageModels;

public class ApplicationsPageModel : PageModelBase, IPageModelFactory<ApplicationsPageModel>
{
    public ApplicationsPageModel(string html) : base(html)
    {
    }

    public static ApplicationsPageModel FromContent(string html) => new(html);

    public string? AgencyNameCaption => _document
        .QuerySelector("h1.govuk-caption-xl")
        ?.TextContent
        .Trim();

    public string? PageHeading => _document
        .QuerySelector("h2.govuk-heading-xl.govuk-\\!-margin-bottom-8")
        ?.TextContent
        .Trim();
}
