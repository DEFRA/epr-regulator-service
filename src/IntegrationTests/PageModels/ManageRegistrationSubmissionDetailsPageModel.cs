namespace IntegrationTests.PageModels;

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

/// <summary>
/// Page Model for the /regulators/registration-submission-details page.
/// Provides an abstraction over AngleSharp for better test readability and maintainability.
/// </summary>
public class ManageRegistrationSubmissionDetailsPageModel : PageModelBase, IPageModelFactory<ManageRegistrationSubmissionDetailsPageModel>
{
    private static readonly Regex GuidInQueryStringRegex = new(
        @"(?:\?|&)(?<key>[^=]+)=(?<id>[0-9a-fA-F]{8}-(?:[0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12})(?:&|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex GuidAnywhereRegex = new(
        @"(?<id>[0-9a-fA-F]{8}-(?:[0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex YearRegex = new(
        @"\b(20\d{2})\b",
        RegexOptions.Compiled);

    private ManageRegistrationSubmissionDetailsPageModel(string html) : base(html)
    {
    }


    public static ManageRegistrationSubmissionDetailsPageModel FromContent(string html) => new (html);

    public string? AgencyNameCaption => (_document
            .QuerySelector("span.govuk-caption-xl")
            ?? _document.QuerySelector("h1.govuk-caption-xl"))
        ?.TextContent
        .Trim();

    public string? PageHeading => (_document
            .QuerySelector("h1.govuk-heading-xl")
            ?? _document.QuerySelector("h2.govuk-heading-xl.govuk-\\!-margin-bottom-8"))
        ?.TextContent
        .Trim();

    /// <summary>
    /// Exposed properties under test - provides a convenient way to access all testable properties.
    /// </summary>
    public RegistrationSubmissionDetailsUnderTest Details => new()
    {
        SubmissionId = SubmissionId,
        OrganisationName = OrganisationName,
        OrganisationType = OrganisationType,
        RegistrationJourneyType = null, // Not rendered on page, validated indirectly through behavior
        RelevantYear = RelevantYear,
        SubmissionDate = SubmissionDate,
        SubmissionStatus = SubmissionStatus
    };

    public Guid? SubmissionId => ExtractSubmissionIdFromAnyLink();

    public string? OrganisationName => _document
        .QuerySelector("h1.govuk-heading-xl")
        ?.TextContent
        .Trim();

    public string? OrganisationType => ExtractOrganisationTypeText();

    public int? RelevantYear => ExtractFirstYearFromPageText();

    public string? SubmissionDate => ExtractSubmissionDateText();

    public string? SubmissionStatus => ExtractSubmissionStatusText();

    // Payment details properties
    public PaymentDetailsUnderTest? PaymentDetails => ExtractPaymentDetails();

    private PaymentDetailsUnderTest? ExtractPaymentDetails()
    {
        // Find the payment details summary card by looking for the card title
        var paymentCard = _document.QuerySelectorAll("div.govuk-summary-card")
            .FirstOrDefault(card => card.QuerySelector("h2.govuk-summary-card__title")?.TextContent?.Contains("Payment") == true);

        if (paymentCard == null)
        {
            return null;
        }

        // Extract amounts from payment-amount rendered <dd> elements
        // The tag helper renders amounts as <dd class="govuk-summary-list__actions"> with £X,XXX.XX format
        var amounts = paymentCard.QuerySelectorAll("dd.govuk-summary-list__actions")
            .Select(dd => dd.TextContent.Trim())
            .Where(text => text.StartsWith("£") || text.Contains("£"))
            .ToList();

        return new PaymentDetailsUnderTest
        {
            HasPaymentSection = true,
            AllAmounts = amounts
        };
    }

    public decimal? ApplicationFee => ExtractPaymentAmount("Application fee");
    public decimal? SubTotal => ExtractPaymentAmount("Sub-total");
    public decimal? PreviousPaymentReceived => ExtractPaymentAmount("Previous payments");
    public decimal? TotalOutstanding => ExtractPaymentAmount("Total outstanding");

    private decimal? ExtractPaymentAmount(string label)
    {
        var paymentCard = _document.QuerySelectorAll("div.govuk-summary-card")
            .FirstOrDefault(card => card.QuerySelector("h2.govuk-summary-card__title")?.TextContent?.Contains("Payment") == true);

        if (paymentCard == null)
        {
            return null;
        }

        // Find rows in the payment card
        var rows = paymentCard.QuerySelectorAll("div.govuk-summary-list__row");
        foreach (var row in rows)
        {
            var labelElement = row.QuerySelector("dd.govuk-summary-list__value strong");
            if (labelElement?.TextContent?.Contains(label, StringComparison.OrdinalIgnoreCase) == true)
            {
                // Get the amount from the actions column (last dd in the row)
                var amountElement = row.QuerySelector("dd.govuk-summary-list__actions");
                if (amountElement != null)
                {
                    return ParseCurrencyAmount(amountElement.TextContent);
                }
            }
        }

        return null;
    }

    private static decimal? ParseCurrencyAmount(string text)
    {
        // Remove currency symbol, whitespace, and parse
        var cleaned = text.Replace("£", "").Replace(",", "").Trim();
        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }
        return null;
    }

    private Guid? ExtractSubmissionIdFromAnyLink()
    {
        var hrefs = _document
            .QuerySelectorAll("a[href]")
            .Select(a => a.GetAttribute("href"))
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .ToList();

        // First, try to find submissionId in query string
        foreach (var href in hrefs)
        {
            var id = ExtractGuidFromQueryString(href!, "submissionId");
            if (id.HasValue)
            {
                return id.Value;
            }
        }

        // Fallback: find any GUID in the href
        foreach (var href in hrefs)
        {
            var id = ExtractFirstGuidAnywhere(href!);
            if (id.HasValue)
            {
                return id.Value;
            }
        }

        return null;
    }

    private static Guid? ExtractGuidFromQueryString(string url, string key)
    {
        var match = GuidInQueryStringRegex.Match(url);
        if (!match.Success)
        {
            return null;
        }

        var matchKey = match.Groups["key"].Value;
        if (!matchKey.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Guid.TryParse(match.Groups["id"].Value, out var guid) ? guid : null;
    }

    private static Guid? ExtractFirstGuidAnywhere(string text)
    {
        var match = GuidAnywhereRegex.Match(text);
        return match.Success && Guid.TryParse(match.Groups["id"].Value, out var guid) ? guid : null;
    }

    private string? ExtractOrganisationTypeText()
    {
        // Best-effort: first "h3 + p.govuk-body" content block is Organisation Type in this view.
        var candidates = _document.QuerySelectorAll("div.govuk-\\!-margin-bottom-4").OfType<IElement>();

        foreach (var div in candidates)
        {
            var h3 = div.QuerySelector("h3.govuk-heading-s");
            var p = div.QuerySelector("p.govuk-body");

            if (h3 is not null && p is not null)
            {
                return p.TextContent.Trim();
            }
        }

        return null;
    }

    private string? ExtractSubmissionStatusText()
    {
        // Try to find the submission status tag within the summary card title wrapper
        // The tag may be a custom element (govuk-tag) or a standard element with .govuk-tag class
        var titleWrapper = _document.QuerySelector("div.govuk-summary-card__title-wrapper");
        if (titleWrapper is not null)
        {
            // Try custom element first (may have content attribute)
            var customTag = titleWrapper.QuerySelector("govuk-tag");
            var content = customTag?.GetAttribute("content")?.Trim();
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            // Try standard element with class
            var standardTag = titleWrapper.QuerySelector(".govuk-tag");
            if (standardTag is not null)
            {
                return standardTag.TextContent.Trim();
            }
        }

        // Fallback: find any .govuk-tag on the page (may pick up phase banner)
        return _document.QuerySelector(".govuk-tag")?.TextContent.Trim();
    }

    private string? ExtractSubmissionDateText()
    {
        var dateText = _document
            .QuerySelector("div.govuk-summary-card__title-wrapper p.govuk-body-s")
            ?.TextContent
            .Trim();

        return string.IsNullOrWhiteSpace(dateText) ? null : dateText;
    }

    private int? ExtractFirstYearFromPageText()
    {
        // Best-effort: find a 4-digit year (e.g., 2024) anywhere on the page.
        var text = _document.Body?.TextContent ?? string.Empty;
        var match = YearRegex.Match(text);

        if (match.Success &&
            int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var year))
        {
            return year;
        }

        return null;
    }
}

public sealed record RegistrationSubmissionDetailsUnderTest
{
    public Guid? SubmissionId { get; init; }
    public string? OrganisationName { get; init; }
    public string? OrganisationType { get; init; }
    public string? RegistrationJourneyType { get; init; }
    public int? RelevantYear { get; init; }
    public string? SubmissionDate { get; init; }
    public string? SubmissionStatus { get; init; }
}

public sealed record PaymentDetailsUnderTest
{
    public bool HasPaymentSection { get; init; }
    public List<string> AllAmounts { get; init; } = new();
}