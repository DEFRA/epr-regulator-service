namespace EPR.RegulatorService.Frontend.Web.Helpers;

public static class RegulatorHelpers
{
    public static string GetRegulatorResourceKey(int nationId) => nationId switch
    {
        2 => "Regulator.NorthernIrelandEnvironmentAgency",
        3 => "Regulator.ScottishEnvironmentProtectionAgency",
        4 => "Regulator.NaturalResourcesWales",
        _ => "Regulator.EnvironmentAgency"
    };
}
