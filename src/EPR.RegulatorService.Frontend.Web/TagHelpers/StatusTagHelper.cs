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
                "granted" => "govuk-tag--green",
                "refused" => "govuk-tag--red",
                "queried" => "govuk-tag--purple",
                "pending" => "govuk-tag--blue",
                "updated" => "govuk-tag--yellow",
                "cancelled" => "govuk-tag--grey",
                "none" => "govuk-tag--purple",
                _ => ""
            }, HtmlEncoder.Default);


            output.Content.AppendHtml($"<span class=\"content\">{Content}</span>");
        }
    }
}