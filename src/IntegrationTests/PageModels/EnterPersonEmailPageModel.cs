namespace IntegrationTests.PageModels;

public class EnterPersonEmailPageModel : PageModelBase, IPageModelFactory<EnterPersonEmailPageModel>
{
    public EnterPersonEmailPageModel(string html) : base(html)
    {
    }

    public static EnterPersonEmailPageModel FromContent(string html) => new(html);

    public IReadOnlyList<string> ErrorSummaryMessages => _document
        .QuerySelectorAll("div.govuk-error-summary a[data-testid='error-banner-text']")
        .Select(a => a.TextContent.Trim())
        .ToList();
}
