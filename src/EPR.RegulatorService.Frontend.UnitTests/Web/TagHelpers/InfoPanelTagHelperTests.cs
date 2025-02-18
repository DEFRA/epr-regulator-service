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
        private static TagHelperContext MakeTagHelperContext() => new(
                tagName: "status-panel",
                allAttributes: [],
                items: new Dictionary<object, object>(),
                uniqueId: "test"
            );

        private static TagHelperOutput MakeTagHelperOutput() => new("status-panel",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

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

        [TestMethod]
        public void Process_WhenResubmissionWithMultipleHeadingsAndContents_GeneratesCorrectHtml()
        {
            // Arrange
            var tagHelper = new StatusPanelTagHelper
            {
                Heading = "Heading 1,Heading 2",
                Content = "Content 1,Content 2",
                Status = "Granted"
            };

            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            tagHelper.Process(context, output);

            // Act
            string result = output.Content.GetContent();

            // Assert
            Assert.IsTrue(result.Contains("<h3>Heading 1</h3><p>Content 1</p><br>"));
            Assert.IsTrue(result.Contains("<h3>Heading 2</h3><p>Content 2</p>"));
        }

        [TestMethod]
        public void Process_WhenSingleHeadingAndContent_GeneratesCorrectHtml()
        {
            // Arrange
            var tagHelper = new StatusPanelTagHelper
            {
                Heading = "Single Heading",
                Content = "Single Content",
                Status = "Pending"
            };

            var context = MakeTagHelperContext();
            var output = MakeTagHelperOutput();

            tagHelper.Process(context, output);

            // Act
            string result = output.Content.GetContent();

            // Assert
            Assert.IsTrue(result.Contains("<h3>Single Heading</h3>"));
            Assert.IsTrue(result.Contains("<p>Single Content</p>"));
        }
    }
}
