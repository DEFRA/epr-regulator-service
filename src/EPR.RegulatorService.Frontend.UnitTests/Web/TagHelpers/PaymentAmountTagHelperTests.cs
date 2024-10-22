namespace EPR.RegulatorService.Frontend.UnitTests.Web.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Web.TagHelpers;

    using Microsoft.AspNetCore.Razor.TagHelpers;

    [TestClass]
    public class PaymentAmountTagHelperTests
    {
        private TagHelperContext MakeTagHelperContext()
        {
            return new TagHelperContext(
                tagName: "payment-amount",
                allAttributes: new TagHelperAttributeList(),
                items: new System.Collections.Generic.Dictionary<object, object>(),
                uniqueId: "test"
            );
        }

        private TagHelperOutput MakeTagHelperOutput()
        {
            return new TagHelperOutput("payment-amount",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        [TestMethod]
        public void Process_SetsTagNameToDd()
        {
            // Arrange
            var tagHelper = new PaymentAmountTagHelper
            {
                Amount = 100.50m
            };
            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            // Act
            tagHelper.Process(context, output);

            // Assert
            Assert.AreEqual("dd", output.TagName);
        }

        [TestMethod]
        public void Process_SetsCorrectAttributes()
        {
            // Arrange
            var tagHelper = new PaymentAmountTagHelper
            {
                Amount = 100.50m
            };
            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            // Act
            tagHelper.Process(context, output);

            // Assert
            Assert.IsTrue(output.Attributes.TryGetAttribute("class", out var classAttribute));
            Assert.AreEqual("govuk-summary-list__actions", classAttribute.Value);

            Assert.IsTrue(output.Attributes.TryGetAttribute("style", out var styleAttribute));
            Assert.AreEqual("text-align: right; flex-grow: 1;", styleAttribute.Value);
        }

        [TestMethod]
        [DataRow(10050, "&#xA3;100.50")] // Represents 100.50
        [DataRow(123456, "&#xA3;1,234.56")] // Represents 1234.56
        [DataRow(99, "&#xA3;0.99")] // Represents 0.99
        public void Process_FormatsAmountAsCurrency(int amountInCents, string expectedFormattedAmount)
        {
            // Arrange
            decimal amount = amountInCents / 100m; // Convert to decimal
            var tagHelper = new PaymentAmountTagHelper
            {
                Amount = amount
            };
            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            // Act
            tagHelper.Process(context, output);

            // Assert
            Assert.AreEqual(expectedFormattedAmount, output.Content.GetContent());
            Assert.IsTrue(output.Content.GetContent().Contains("&#xA3;"), "Output should contain the pound symbol as an HTML entity.");
        }
    }
}
