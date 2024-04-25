namespace EPR.RegulatorService.Frontend.UnitTests.Core.Converters;

using System.Text.Json;
using Frontend.Core.Converters;
using Frontend.Core.Enums;

[TestClass]
public class OrganisationTypeConverterTests
{
    private JsonSerializerOptions _options;

    [TestInitialize]
    public void TestInitialize()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new OrganisationTypeConverter() }
        };
    }

    [TestMethod]
    public void Deserialize_DirectProducer_ShouldReturnDirectProducerEnum()
    {
        string json = "\"Direct Producer\"";

        var result = JsonSerializer.Deserialize<OrganisationType>(json, _options);

        result.Should().Be(OrganisationType.DirectProducer);
    }

    [TestMethod]
    public void Deserialize_ComplianceScheme_ShouldReturnComplianceSchemeEnum()
    {
        string json = "\"Compliance Scheme\"";

        var result = JsonSerializer.Deserialize<OrganisationType>(json, _options);

        result.Should().Be(OrganisationType.ComplianceScheme);
    }

    [TestMethod]
    public void Deserialize_InvalidString_ShouldThrowJsonException()
    {
        string json = "\"Invalid String\"";

        Action act = () => JsonSerializer.Deserialize<OrganisationType>(json, _options);

        act.Should().Throw<JsonException>()
            .WithMessage("*Value 'Invalid String' is not valid for OrganisationType*");
    }

    [TestMethod]
    public void Serialize_DirectProducer_ShouldReturnCorrectString()
    {
        var enumValue = OrganisationType.DirectProducer;

        var json = JsonSerializer.Serialize(enumValue, _options);

        json.Should().Be("\"Direct Producer\"");
    }

    [TestMethod]
    public void Serialize_ComplianceScheme_ShouldReturnCorrectString()
    {
        var enumValue = OrganisationType.ComplianceScheme;

        var json = JsonSerializer.Serialize(enumValue, _options);

        json.Should().Be("\"Compliance Scheme\"");
    }
}