using Microsoft.Extensions.Configuration;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.FeatureManagement;

[TestClass]
public class FeatureFlagTests
{
    private IConfiguration configuration;

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ShowYourFeedbackFooter_Should_Be_True(bool value)
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string>
        {
            {EPR.RegulatorService.Frontend.Web.Configs.FeatureFlags.ShowYourFeedbackFooter, value.ToString()}
        };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        bool showYourFeedbackFooter = bool.Parse(configuration[EPR.RegulatorService.Frontend.Web.Configs.FeatureFlags.ShowYourFeedbackFooter]);

        // Assert
        showYourFeedbackFooter.Should().Be(value);
    }
}
