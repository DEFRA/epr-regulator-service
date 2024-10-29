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
            string statusClassName = Status switch
            {
                "granted" => "info-panel__granted",
                "refused" => "info-panel__refused",
                "pending" => "info-panel__pending",
                "cancelled" => "info-panel__cancelled",
                "queried" => "info-panel__queried",
                "updated" => "info-panel__updated",
                _ => ""
            };

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"info-panel {statusClassName}");
            output.Content.AppendHtml($"<h3>{Heading}</h3><p>{Content}</p>");
        }
    }
}