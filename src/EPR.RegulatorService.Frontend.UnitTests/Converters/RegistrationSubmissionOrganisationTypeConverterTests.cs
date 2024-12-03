using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.RegulatorService.Frontend.Core.Converters;
using EPR.RegulatorService.Frontend.Core.Enums;
using System.Text.Json;
using System.Text;

namespace EPR.RegulatorService.Frontend.Core.Converters.Tests;

[TestClass()]
public class RegistrationSubmissionOrganisationTypeConverterTests
{
    private RegistrationSubmissionOrganisationTypeConverter _converter;
    private JsonSerializerOptions _options;

    [TestInitialize]
    public void Setup()
    {
        _converter = new RegistrationSubmissionOrganisationTypeConverter();
        _options = new JsonSerializerOptions();
    }

    [TestMethod]
    public void Read_ValidEnumValue_ReturnsCorrectEnum()
    {
        // Arrange
        string json = "\"compliance\"";
        Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        reader.Read(); // Move to the value

        // Act
        var result = _converter.Read(ref reader, typeof(RegistrationSubmissionOrganisationType), _options);

        // Assert
        Assert.AreEqual(RegistrationSubmissionOrganisationType.compliance, result);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Read_InvalidEnumValue_ThrowsJsonException()
    {
        // Arrange
        string json = "\"InvalidType\"";
        Utf8JsonReader reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        reader.Read(); // Move to the value

        // Act
        _converter.Read(ref reader, typeof(RegistrationSubmissionOrganisationType), _options);

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
        _converter.Read(ref reader, typeof(RegistrationSubmissionOrganisationType), _options);

        // Assert
        // Exception is expected
    }

    [TestMethod]
    public void Write_ValidEnumValue_WritesCorrectStringValue()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var writer = new Utf8JsonWriter(memoryStream);

        var value = RegistrationSubmissionOrganisationType.large;

        // Act
        _converter.Write(writer, value, _options);
        writer.Flush();

        string resultJson = Encoding.UTF8.GetString(memoryStream.ToArray());

        // Assert
        Assert.AreEqual("\"large\"", resultJson);
    }

    [TestMethod]
    public void Write_AnotherValidEnumValue_WritesCorrectStringValue()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var writer = new Utf8JsonWriter(memoryStream);

        var value = RegistrationSubmissionOrganisationType.small;

        // Act
        _converter.Write(writer, value, _options);
        writer.Flush();

        string resultJson = Encoding.UTF8.GetString(memoryStream.ToArray());

        // Assert
        Assert.AreEqual("\"small\"", resultJson);
    }
}