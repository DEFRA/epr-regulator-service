namespace EPR.RegulatorService.Frontend.Core.Extensions;

public static class BoolExtensionMethods
{
    public static string ToYesNo(this bool inBool) => inBool ? "Yes" : "No";
}
