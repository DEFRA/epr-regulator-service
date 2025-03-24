using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace EPR.RegulatorService.Frontend.Web.TagHelpers
{
    [HtmlTargetElement("govuk-tag", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class StatusTagHelper(IStringLocalizer<SharedResources> sharedLocalizer) : TagHelper
    {
        private const string ContentAttributeName = "content";
        private const string StatusAttributeName = "status";
        private const string UseLightColourAttributeName = "useLightColour";
        private const string UseResubmissionPrefixAttributeName = "useResubmissionPrefix";

        public IStringLocalizer<SharedResources> SharedLocalizer { get; } = sharedLocalizer;

        [HtmlAttributeName(ContentAttributeName)]
        public string? Content { get; set; }

        [HtmlAttributeName(StatusAttributeName)]
        public string? Status { get; set; }

        [HtmlAttributeName(UseLightColourAttributeName)]
        public bool UseLightColour { get; set; }

        [HtmlAttributeName(UseResubmissionPrefixAttributeName)]
        public bool UseResubmissionPrefix { get; set; }

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
                "Pending" => UseLightColour ? "govuk-tag--light-blue" : "govuk-tag--blue",
                "Updated" => "govuk-tag--yellow",
                "Cancelled" => "status__cancelled",
                "Accepted" => "govuk-tag--light-blue",
                "Rejected" => "govuk-tag--light-blue",
                _ => string.Empty
            }, HtmlEncoder.Default);

            if (!string.IsNullOrWhiteSpace(Status))
            {
                Content = (UseResubmissionPrefix, Status) switch
                {
                    (true, "Pending") => SharedLocalizer["Resubmission.Pending"],
                    (true, "Accepted") => SharedLocalizer["Resubmission.Accepted"],
                    (true, "Rejected") => SharedLocalizer["Resubmission.Rejected"],
                    _ => Content
                };

                output.Content.AppendHtml($"<span class=\"content\">{Content}</span>");
            }
        }
    }
}
