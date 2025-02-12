using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

namespace EPR.RegulatorService.Frontend.Web.Helpers
{
    [HtmlTargetElement("reference-number-panel", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ReferenceNumberPanelTagHelper : TagHelper
    {
        private const string ModelAttributeName = "model";

        private readonly IStringLocalizer _localizer;

        public ReferenceNumberPanelTagHelper(IStringLocalizerFactory localizerFactory)
        {
            var type = typeof(SharedResources);
            _localizer = localizerFactory.Create(type);
        }

        [HtmlAttributeName(ModelAttributeName)]
        public RegistrationSubmissionDetailsViewModel Model { get; set; } = null!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.AddClass("info-panel", HtmlEncoder.Default);
            output.AddClass("info-panel__granted", HtmlEncoder.Default);

            var contentBuilder = new System.Text.StringBuilder();

            if (Model.IsResubmission)
            {
                contentBuilder.AppendLine($"<h3>{_localizer["RegistrationSubmissionDetails.RegistrationReferenceNumber"].Value}</h3>");
                contentBuilder.AppendLine($"<p>{Model.RegistrationReferenceNumber}</p>");
                contentBuilder.AppendLine($"<h3>{_localizer["RegistrationSubmissionDetails.ApplicationReferenceNumber"].Value}</h3>");
                contentBuilder.AppendLine($"<p>{Model.ReferenceNumber}</p>");
            }
            else
            {
                string titleKey = !string.IsNullOrWhiteSpace(Model.RegistrationReferenceNumber)
                    ? _localizer["RegistrationSubmissionDetails.RegistrationReferenceNumber"]
                    : _localizer["RegistrationSubmissionDetails.ApplicationReferenceNumber"];

                string referenceNumber = !string.IsNullOrWhiteSpace(Model.RegistrationReferenceNumber)
                    ? Model.RegistrationReferenceNumber
                    : Model.ReferenceNumber;

                contentBuilder.AppendLine($"<h3>{_localizer[titleKey].Value}</h3>");
                contentBuilder.AppendLine($"<p>{referenceNumber}</p>");
            }

            output.Content.SetHtmlContent(contentBuilder.ToString());
        }
    }
}
