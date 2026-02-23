namespace IntegrationTests.PageModels;

public class HomePageModel : PageModelBase, IPageModelFactory<HomePageModel>
{
    public HomePageModel(string html) : base(html)
    {
    }

    public static HomePageModel FromContent(string html) => new(html);

    public string? OrganisationName => _document
        .QuerySelector("h2.govuk-heading-l.govuk-\\!-margin-bottom-2")
        ?.TextContent
        .Trim();

    public string? PersonName => _document
        .QuerySelector("h3.govuk-heading-m.govuk-caption-l.govuk-\\!-margin-bottom-2")
        ?.TextContent
        .Trim();

    public string? PageHeading => _document
        .QuerySelector("h1.govuk-heading-xl.govuk-\\!-margin-bottom-4")
        ?.TextContent
        .Trim();
}
