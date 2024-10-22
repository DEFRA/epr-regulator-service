namespace EPR.RegulatorService.Frontend.Web.TagHelpers
{
    using System.Globalization;

    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("payment-amount")]
    public class PaymentAmountTagHelper : TagHelper
    {
        public decimal Amount { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Ensure the tag is <dd> with the correct class and style
            output.TagName = "dd";
            output.Attributes.SetAttribute("class", "govuk-summary-list__actions");
            output.Attributes.SetAttribute("style", "text-align: right; flex-grow: 1;");

            // Format the amount as currency with commas and 2 decimal places
            string formattedAmount = string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", Amount);

            // Set the content to the formatted amount
            output.Content.SetContent(formattedAmount);
        }
    }
}
