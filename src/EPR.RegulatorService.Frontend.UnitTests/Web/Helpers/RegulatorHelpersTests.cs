namespace EPR.RegulatorService.Frontend.UnitTests.Web.Helpers;

using Frontend.Web.Helpers;

[TestClass]
public class RegulatorHelpersTests
{
    // Redundant unit test (already covered by integration tests) just to satisfy sonarqube ruleset
    [TestMethod]
    [DataRow(1, "Regulator.EnvironmentAgency")]
    [DataRow(2, "Regulator.NorthernIrelandEnvironmentAgency")]
    [DataRow(3, "Regulator.ScottishEnvironmentProtectionAgency")]
    [DataRow(4, "Regulator.NaturalResourcesWales")]
    public void GetRegulatorResourceKey_ReturnsExpectedKey(int nationId, string expectedKey) => RegulatorHelpers.GetRegulatorResourceKey(nationId).Should().Be(expectedKey);
}
