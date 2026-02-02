using System;
using System.Text.Json;

using EPR.RegulatorService.Frontend.Core.Converters;
using EPR.RegulatorService.Frontend.Core.Enums;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Converters
{
    [TestClass]
    public class RegistrationJourneyTypeConverterTests
    {
        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new RegistrationJourneyTypeConverter());
            return options;
        }

        [TestMethod]
        public void Read_ValidEnumString_ReturnsEnum()
        {
            var options = CreateOptions();
            var json = "\"CsoLargeProducer\"";

            var result = JsonSerializer.Deserialize<RegistrationJourneyType>(json, options);

            Assert.AreEqual(RegistrationJourneyType.CsoLargeProducer, result);
        }

        [TestMethod]
        public void Write_Enum_WritesString()
        {
            var options = CreateOptions();
            var value = RegistrationJourneyType.CsoSmallProducer;

            var json = JsonSerializer.Serialize(value, options);

            Assert.AreEqual("\"CsoSmallProducer\"", json);
        }

        [TestMethod]
        public void Read_InvalidString_ThrowsJsonException()
        {
            var options = CreateOptions();
            var json = "\"NotAValidJourney\"";

            Assert.ThrowsException<JsonException>(() => JsonSerializer.Deserialize<RegistrationJourneyType>(json, options));
        }

        [TestMethod]
        public void RoundTrip_AllEnumValues_SerializeAndDeserializeMatch()
        {
            var options = CreateOptions();

            foreach (RegistrationJourneyType expected in Enum.GetValues(typeof(RegistrationJourneyType)))
            {
                var json = JsonSerializer.Serialize(expected, options);
                var actual = JsonSerializer.Deserialize<RegistrationJourneyType>(json, options);

                Assert.AreEqual(expected, actual);
            }
        }
    }
}