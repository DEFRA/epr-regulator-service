namespace EPR.RegulatorService.Frontend.UnitTests.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;

    [TestClass]
    public class RegistrationSubmissionsFilterModelTests
    {
        private RegistrationSubmissionsFilterModel _sut;

        [TestInitialize]
        public void SetUp()
        {
            _sut = new();
        }

        [TestMethod]
        public void SetOrganisationName_RemovesArticles_AndOtherTerms()
        {
            // Arrange
            string input = "The Best & Bright Solutions for You";
            string expected = "Best Bright Solutions You";  // Expected result after filtering

            // Act
            _sut.OrganisationName = input;

            // Assert
            Assert.AreEqual(expected, _sut.OrganisationName);
        }

        [TestMethod]
        public void SetOrganisationName_MixedCase_RemovesArticles_AndOtherTerms()
        {
            // Arrange
            string input = "A Great Invention and the Best Solutions";
            string expected = "Great Invention Best Solutions";  // Expected result after filtering

            // Act
            _sut.OrganisationName = input;

            // Assert
            Assert.AreEqual(expected, _sut.OrganisationName);
        }

        [TestMethod]
        public void SetOrganisationName_NoArticlesOrTerms_ReturnsOriginal()
        {
            // Arrange
            string input = "Innovative Solutions Ltd";
            string expected = "Innovative Solutions Ltd";  // No terms to filter

            // Act
            _sut.OrganisationName = input;

            // Assert
            Assert.AreEqual(expected, _sut.OrganisationName);
        }

        [TestMethod]
        public void SetOrganisationName_EmptyInput_ReturnsEmptyString()
        {
            // Arrange
            string input = "";
            string expected = "";  // Expected to return empty

            // Act
            _sut.OrganisationName = input;

            // Assert
            Assert.AreEqual(expected, _sut.OrganisationName);
        }

        [TestMethod]
        public void SetOrganisationName_NullInput_ReturnsNull()
        {
            // Arrange
            string input = null;
            string expected = null;  // Should handle null gracefully

            // Act
            _sut.OrganisationName = input;

            // Assert
            Assert.AreEqual(expected, _sut.OrganisationName);
        }
    }
}
