namespace IntegrationTests.PageModels;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

public abstract class PageModelBase
{
    protected readonly IHtmlDocument _document;

    protected PageModelBase(string html)
    {
        var parser = new HtmlParser();
        _document = parser.ParseDocument(html);
    }
}

public interface IPageModelFactory<out T> where T : PageModelBase
{
    static abstract T FromContent(string html);
}
