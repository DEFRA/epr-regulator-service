namespace EPR.RegulatorService.Frontend.Web.Constants.ReprocessorExporter;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class PermitTypes
{
    // TODO: These spellings are incorrect ("Contril", "License") but need to be corrected in the ADR, Facade and BE before changing here.
    public const string WasteExemption = "Waste Exemption";
    public const string PollutionPreventionAndControlPermit = "Pollution,Prevention and Contril(PPC) permit";
    public const string WasteManagementLicence = "Waste Management License";
    public const string InstallationPermit = "Installation Permit";
    public const string EnvironmentalPermitOrWasteManagementLicence = "Environmental permit or waste management license";
}