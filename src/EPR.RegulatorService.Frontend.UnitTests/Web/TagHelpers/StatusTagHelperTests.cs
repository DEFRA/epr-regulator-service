using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EPR.RegulatorService.Frontend.UnitTests.TagHelpers;

[TestClass]
public class StatusTagHelperTests
{
    private static StatusTagHelper CreateTagHelper(string content = "Test", string status = "granted")
    {
        return new StatusTagHelper
        {
            Content = content,
            Status = status
        };
    }

    [TestMethod]
    public void Process_ShouldSetTagNameToSpan()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.AreEqual("span", output.TagName);
    }

    [TestMethod]
    public void Process_ShouldAddGovukTagClass()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["class"].Value.ToString().Should().Contain("govuk-tag");
    }

    [DataTestMethod]
    [DataRow("granted", "govuk-tag--green")]
    [DataRow("refused", "govuk-tag--red")]
    [DataRow("queried", "govuk-tag--purple")]
    [DataRow("pending", "govuk-tag--blue")]
    [DataRow("updated", "govuk-tag--yellow")]
    [DataRow("cancelled", "status__cancelled")]
    [DataRow("none", "govuk-tag--purple")]
    public void Process_ShouldAddCorrectStatusClass(string status, string expectedClass)
    {
        // Arrange
        var tagHelper = CreateTagHelper(status: status);
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["class"].Value.ToString().Should().Contain(expectedClass);
    }

    [TestMethod]
    public void Process_ShouldAddContentToSpan()
    {
        // Arrange
        var tagHelper = CreateTagHelper(content: "Test Content");
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Content.GetContent().Should().Contain("Test Content");
    }

    [TestMethod]
    public void Process_ShouldHandleNullStatus()
    {
        // Arrange
        var tagHelper = CreateTagHelper(status: null);
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        output.Attributes["class"].Value.ToString().Should().NotContain("govuk-tag--");
    }
}