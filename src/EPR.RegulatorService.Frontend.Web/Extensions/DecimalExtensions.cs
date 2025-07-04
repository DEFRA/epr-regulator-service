namespace EPR.RegulatorService.Frontend.Web.Extensions;

public static class DecimalExtensions
{
    public static string ToDisplayNumber(this decimal input) =>
        $"{input:N2}".TrimEnd('0').TrimEnd('.');

    public static string ToDisplayNumber(this decimal? input) =>
        input == null
            ? string.Empty
            : ToDisplayNumber(input.Value);

    public static string ToDisplayCurrency(this decimal input, bool showDecimals = true) =>
        showDecimals ?
            $"£{input:N2}":
            $"£{input:N0}";
}
