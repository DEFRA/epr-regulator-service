using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EPR.RegulatorService.Frontend.Web.TagHelpers
{
    [HtmlTargetElement("companies-house-link", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class CompaniesHouseTagHelper : TagHelper
    {
        private const string ValueAttributeName = "gov-value";

        [HtmlAttributeName(ValueAttributeName)]
        public string? Value { get; set; }
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", "https://find-and-update.company-information.service.gov.uk/company/" + Value);
            output.Attributes.SetAttribute("target", "_default");
        }
    }
}
