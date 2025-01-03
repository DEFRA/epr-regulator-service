namespace EPR.RegulatorService.Frontend.UnitTests.Web.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EPR.RegulatorService.Frontend.Web.TagHelpers;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [TestClass]
    public class InfoPanelTagHelperTests
    {
        private static TagHelperContext MakeTagHelperContext()
        {
            return new TagHelperContext(
                tagName: "status-panel",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test"
            );
        }

        private static TagHelperOutput MakeTagHelperOutput()
        {
            return new TagHelperOutput("status-panel",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        [TestMethod]
        [DataRow("Granted", "info-panel info-panel__granted")]
        [DataRow("Refused", "info-panel info-panel__refused")]
        [DataRow("Pending", "info-panel info-panel__pending")]
        [DataRow("Cancelled", "info-panel info-panel__cancelled")]
        [DataRow("Queried", "info-panel info-panel__queried")]
        [DataRow("Updated", "info-panel info-panel__updated")]
        [DataRow(null, "info-panel ")]
        public void Process_StatusAttribute_GeneratesCorrectClass(string status, string expectedClass)
        {
            // Arrange
            var tagHelper = new StatusPanelTagHelper
            {
                Heading = "Test Heading",
                Content = "Test Content",
                Status = status
            };

            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            // Act
            tagHelper.Process(context, output);

            // Assert
            Assert.AreEqual("div", output.TagName);
            Assert.AreEqual(TagMode.StartTagAndEndTag, output.TagMode);
            Assert.IsTrue(output.Attributes.TryGetAttribute("class", out var classAttribute));
            Assert.AreEqual(expectedClass, classAttribute.Value);
            Assert.IsTrue(output.Content.GetContent().Contains("<h3>Test Heading</h3>"));
            Assert.IsTrue(output.Content.GetContent().Contains("<p>Test Content</p>"));
        }
    }
}
