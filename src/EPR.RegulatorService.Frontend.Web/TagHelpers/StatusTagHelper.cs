using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EPR.RegulatorService.Frontend.Web.TagHelpers
{
    [HtmlTargetElement("govuk-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class StatusTagHelper : TagHelper
    {
        private const string ContentAttributeName = "content";
        private const string StatusAttributeName = "status";

        [HtmlAttributeName(ContentAttributeName)]
        public string? Content { get; set; }

        [HtmlAttributeName(StatusAttributeName)]
        public string? Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;

            output.TagName = "span";
            output.AddClass("govuk-tag", HtmlEncoder.Default);

            output.AddClass(Status switch
            {
                "Granted" => "govuk-tag--green",
                "Refused" => "govuk-tag--red",
                "Queried" => "govuk-tag--purple",
                "Pending" => "govuk-tag--blue",
                "Updated" => "govuk-tag--yellow",
                "Cancelled" => "status__cancelled",
                _ => string.Empty
            }, HtmlEncoder.Default);


            output.Content.AppendHtml($"<span class=\"content\">{Content}</span>");
        }
    }
}
