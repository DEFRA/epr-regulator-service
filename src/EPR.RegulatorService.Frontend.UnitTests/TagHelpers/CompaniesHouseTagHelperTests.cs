using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EPR.RegulatorService.Frontend.Web.TagHelpers.Tests
{
    [TestClass]
    public class CompaniesHouseTagHelperTests
    {
        [TestMethod]
        public void Process_WithValidValue_SetsTagNameAndAttributes()
        {
            // Arrange
            var tagHelper = new CompaniesHouseTagHelper
            {
                Value = "12345678"
            };

            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var tagHelperOutput = new TagHelperOutput("companies-house-link",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder)
                    => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(tagHelperContext, tagHelperOutput);

            // Assert
            Assert.AreEqual("a", tagHelperOutput.TagName);
            Assert.IsTrue(tagHelperOutput.Attributes.ContainsName("href"));
            Assert.AreEqual("https://find-and-update.company-information.service.gov.uk/company/12345678",
                tagHelperOutput.Attributes["href"].Value);
            Assert.IsTrue(tagHelperOutput.Attributes.ContainsName("target"));
            Assert.AreEqual("_default", tagHelperOutput.Attributes["target"].Value);
        }

        [TestMethod]
        public void Process_WithNullValue_SetsTagNameAndAttributesWithoutCompanyNumber()
        {
            // Arrange
            var tagHelper = new CompaniesHouseTagHelper
            {
                Value = null
            };

            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test2");

            var tagHelperOutput = new TagHelperOutput("companies-house-link",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder)
                    => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(tagHelperContext, tagHelperOutput);

            // Assert
            Assert.AreEqual("a", tagHelperOutput.TagName);
            Assert.IsTrue(tagHelperOutput.Attributes.ContainsName("href"));
            // Even if Value is null, it will just end with 'company/'
            Assert.AreEqual("https://find-and-update.company-information.service.gov.uk/company/",
                tagHelperOutput.Attributes["href"].Value);
            Assert.IsTrue(tagHelperOutput.Attributes.ContainsName("target"));
            Assert.AreEqual("_default", tagHelperOutput.Attributes["target"].Value);
        }

        [TestMethod]
        public void Process_WithEmptyValue_SetsTagNameAndAttributesWithoutCompanyNumber()
        {
            // Arrange
            var tagHelper = new CompaniesHouseTagHelper
            {
                Value = "" // empty string
            };

            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test3");

            var tagHelperOutput = new TagHelperOutput("companies-house-link",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder)
                    => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(tagHelperContext, tagHelperOutput);

            // Assert
            Assert.AreEqual("a", tagHelperOutput.TagName);
            Assert.IsTrue(tagHelperOutput.Attributes.ContainsName("href"));
            Assert.AreEqual("https://find-and-update.company-information.service.gov.uk/company/",
                tagHelperOutput.Attributes["href"].Value);
            Assert.AreEqual("_default", tagHelperOutput.Attributes["target"].Value);
        }
    }
}