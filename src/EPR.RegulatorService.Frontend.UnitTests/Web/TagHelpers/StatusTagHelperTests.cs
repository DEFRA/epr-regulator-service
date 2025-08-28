using EPR.RegulatorService.Frontend.Web;
using EPR.RegulatorService.Frontend.Web.TagHelpers;

using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace EPR.RegulatorService.Frontend.UnitTests.TagHelpers;

[TestClass]
public class StatusTagHelperTests
{
    private static StatusTagHelper CreateTagHelper(
        string content = "Test",
        string status = "Granted",
        bool useLightColour = false,
        bool useResubmissionPrefix = false,
        IStringLocalizer<SharedResources> sharedLocalizer = null)
    {
        sharedLocalizer ??= new Mock<IStringLocalizer<SharedResources>>().Object;

        return new StatusTagHelper(sharedLocalizer)
        {
            Content = content,
            Status = status,
            UseLightColour = useLightColour,
            UseResubmissionPrefix = useResubmissionPrefix
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
    [DataRow("Granted", "govuk-tag--green")]
    [DataRow("Refused", "govuk-tag--red")]
    [DataRow("Queried", "govuk-tag--purple")]
    [DataRow("Pending", "govuk-tag--blue")]
    [DataRow("Updated", "govuk-tag--yellow")]
    [DataRow("Cancelled", "status__cancelled")]
    [DataRow("Accepted", "govuk-tag--light-blue")]
    [DataRow("Rejected", "govuk-tag--light-blue")]
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

    [TestMethod]
    public void Process_ShouldUseLightBlueForPending_WhenUseLightColourIsTrue()
    {
        // Arrange
        var tagHelper = CreateTagHelper(status: "Pending", useLightColour: true);
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.IsTrue(output.Attributes["class"].Value.ToString().Contains("govuk-tag--light-blue"));
    }

    [TestMethod]
    public void Process_ShouldUseBlueForPending_WhenUseLightColourIsFalse()
    {
        // Arrange
        var tagHelper = CreateTagHelper(status: "Pending", useLightColour: false);
        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.IsTrue(output.Attributes["class"].Value.ToString().Contains("govuk-tag--blue"));
    }

    [TestMethod]
    [DataRow("Pending")]
    [DataRow("Accepted")]
    [DataRow("Rejected")]
    public void Process_ShouldUseResubmissionPrefix_WhenUseResubmissionPrefixIsTrue(string resubmissionStatus)
    {
        // Arrange
        var mockLocalizer = new Mock<IStringLocalizer<SharedResources>>();
        mockLocalizer.Setup(ml => ml["Resubmission.Pending"]).Returns(new LocalizedString("Resubmission.Pending", "Resubmission Pending"));
        mockLocalizer.Setup(ml => ml["Resubmission.Accepted"]).Returns(new LocalizedString("Resubmission.Accepted", "Resubmission Accepted"));
        mockLocalizer.Setup(ml => ml["Resubmission.Rejected"]).Returns(new LocalizedString("Resubmission.Rejected", "Resubmission Rejected"));

        var tagHelper = CreateTagHelper(
            status: resubmissionStatus,
            useResubmissionPrefix: true,
            sharedLocalizer: mockLocalizer.Object);

        var context = new TagHelperContext([], new Dictionary<object, object>(), Guid.NewGuid().ToString());
        var output = new TagHelperOutput("govuk-tag", [], (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.IsTrue(output.Content.GetContent().Contains($"Resubmission {resubmissionStatus}"));
    }
}