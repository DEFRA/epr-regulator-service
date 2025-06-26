namespace EPR.RegulatorService.Frontend.Web.TagHelpers.ManageRegistrationSubmissions
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("resubmission-reference-number-panel", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ResubmissionReferenceNumbersPanelTagHelper : TagHelper
    {
        private const string ProducerRegistrationReferenceHeadingAttributeName = "producerRegistrationReference";
        private const string ApplicationReferenceHeadingAttributeName = "applicationReference";
        private const string ProducerRegistrationReferenceContentAttributeName = "producerRegistrationReferenceContent";
        private const string ApplicationReferenceHeadingContentAttributeName = "applicationReferenceContent";
        private const string StatusAttributeName = "status";

        [HtmlAttributeName(ProducerRegistrationReferenceHeadingAttributeName)]
        public string? ProducerRegistrationReferenceHeading { get; set; }

        [HtmlAttributeName(ProducerRegistrationReferenceContentAttributeName)]
        public string? ProducerRegistrationReferenceContent { get; set; }

        [HtmlAttributeName(ApplicationReferenceHeadingAttributeName)]
        public string? ApplicationReferenceHeading { get; set; }

        [HtmlAttributeName(ApplicationReferenceHeadingContentAttributeName)]
        public string? ApplicationReferenceHeadingContent { get; set; }

        [HtmlAttributeName(StatusAttributeName)]
        public string? Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
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
            output.Content.AppendHtml($"<h3>{ProducerRegistrationReferenceHeading}</h3><p>{ProducerRegistrationReferenceContent}</p>");
            output.Content.AppendHtml("<br>");
            output.Content.AppendHtml($"<h3>{ApplicationReferenceHeading}</h3><p>{ApplicationReferenceHeadingContent}</p>");
        }
    }
}
