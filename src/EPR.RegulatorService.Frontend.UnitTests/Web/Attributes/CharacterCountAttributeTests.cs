using EPR.RegulatorService.Frontend.Web.Attributes;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Attributes
{
    [TestClass]
    public class CharacterCountAttributeTests
    {
        private const string RequiredErrorMessage = "Testing Required Error Message";
        private const string LengthExceededErrorMessage = "Testing Exceeded Error Message";
        private const int MaxLength = 10;
        private CharacterCountAttribute _systemUnderTest = null!;

        [TestInitialize]
        public void Setup()
        {
            _systemUnderTest = new CharacterCountAttribute(RequiredErrorMessage, LengthExceededErrorMessage, MaxLength);
        }

        [TestMethod]
        [DataRow("Testing String", false)]
        [DataRow("Testing", true)]
        public void GivenAString_ThenReturnValidationResult(string value, bool expected)
        {
            // Arrange

            // Act
            bool validState = _systemUnderTest.IsValid(value);

            // Assert
            Assert.IsNotNull(validState);
            Assert.AreEqual(expected: expected, actual: validState);
        }

        [TestMethod]        
        public void GivenNull_ThenReturnValidationResult()
        {
            // Arrange

            // Act
            bool validState = _systemUnderTest.IsValid(null);

            // Assert
            Assert.IsNotNull(validState);
            Assert.IsFalse(validState);
        }
    }
}