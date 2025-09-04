using EPR.RegulatorService.Frontend.Core.Enums;
using System.Text.Json;
using System.Text;

namespace EPR.RegulatorService.Frontend.Core.Converters.Tests
{
    [TestClass()]
    public class RegistrationSubmissionStatusConverterTests
    {
        private RegistrationSubmissionStatusConverter _converter;
        private JsonSerializerOptions _options;

        [TestInitialize]
        public void Setup()
        {
            _converter = new RegistrationSubmissionStatusConverter();
            _options = new JsonSerializerOptions();
        }

        [TestMethod]
        public void Read_ValidEnumValue_ReturnsCorrectEnum()
        {
            // Arrange
            string json = "\"Pending\"";
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the value

            // Act
            var result = _converter.Read(ref reader, typeof(RegistrationSubmissionStatus), _options);

            // Assert
            Assert.AreEqual(RegistrationSubmissionStatus.Pending, result);
        }

        [TestMethod]
        public void Read_ValidEnumValueWithDifferentCasing_ReturnsCorrectEnum()
        {
            // Arrange
            string json = "\"granted\"";
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the value

            // Act
            var result = _converter.Read(ref reader, typeof(RegistrationSubmissionStatus), _options);

            // Assert
            Assert.AreEqual(RegistrationSubmissionStatus.Granted, result);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Read_InvalidEnumValue_ThrowsJsonException()
        {
            // Arrange
            string json = "\"InvalidStatus\"";
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the value

            // Act
            _converter.Read(ref reader, typeof(RegistrationSubmissionStatus), _options);

            // Assert
            // Exception is expected
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public void Read_EmptyValue_ThrowsJsonException()
        {
            // Arrange
            string json = "\"\"";
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the value

            // Act
            _converter.Read(ref reader, typeof(RegistrationSubmissionStatus), _options);

            // Assert
            // Exception is expected
        }

        [TestMethod]
        public void Write_ValidEnumValue_WritesCorrectStringValue()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(memoryStream);

            var value = RegistrationSubmissionStatus.Queried;

            // Act
            _converter.Write(writer, value, _options);
            writer.Flush();

            string resultJson = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Assert
            Assert.AreEqual("\"Queried\"", resultJson);
        }

        [TestMethod]
        public void Write_AnotherValidEnumValue_WritesCorrectStringValue()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(memoryStream);

            var value = RegistrationSubmissionStatus.Cancelled;

            // Act
            _converter.Write(writer, value, _options);
            writer.Flush();

            string resultJson = Encoding.UTF8.GetString(memoryStream.ToArray());

            // Assert
            Assert.AreEqual("\"Cancelled\"", resultJson);
        }

        [TestMethod]
        public void Read_EnumValueWithSpacesAndDescription_ReturnsCorrectEnum()
        {
            // Arrange
            string json = "\"Updated\"";
            Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the value

            // Act
            var result = _converter.Read(ref reader, typeof(RegistrationSubmissionStatus), _options);

            // Assert
            Assert.AreEqual(RegistrationSubmissionStatus.Updated, result);
        }
    }
}