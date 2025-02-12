using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EPR.RegulatorService.Frontend.Web.TagHelpers
{
    [HtmlTargetElement("status-panel", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class StatusPanelTagHelper : TagHelper
    {
        private const string HeadingAttributeName = "heading";
        private const string ContentAttributeName = "content";
        private const string StatusAttributeName = "status";

        [HtmlAttributeName(HeadingAttributeName)]
        public string? Heading { get; set; }

        [HtmlAttributeName(ContentAttributeName)]
        public string? Content { get; set; }

        [HtmlAttributeName(StatusAttributeName)]
        public string? Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Handle multiple values in Heading and Content for resubmission
            string[] headings = Heading?.Split(',') ?? [];
            string[] contents = Content?.Split(',') ?? [];

            string statusClassName = Status switch
            {
                "Granted" => "info-panel__granted",
                "Refused" => "info-panel__refused",
                "Pending" => "info-panel__pending",
                "Cancelled" => "info-panel__cancelled",
                "Queried" => "info-panel__queried",
                "Updated" => "info-panel__updated",
                _ => string.Empty
            };

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"info-panel {statusClassName}");

            // If the status is granted and resubmission is required
            if (Status == "Granted" && headings.Length > 1 && contents.Length > 1)
            {
                output.Content.AppendHtml($"<h3>{headings[0]}</h3><p>{contents[0]}</p><br>");
                output.Content.AppendHtml($"<h3>{headings[1]}</h3><p>{contents[1]}</p>");
            }
            else
            {
                output.Content.AppendHtml($"<h3>{headings[0]}</h3><p>{contents[0]}</p>");
            }
        }
    }
}