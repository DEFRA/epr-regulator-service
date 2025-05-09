namespace EPR.RegulatorService.Frontend.Web.Configs
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class RegistrationSubmissionsOptions
    {
        public const string ConfigSection = "BehaviourManagement:RegistrationSubmissions";

        public bool Show2026RelevantYearFilter { get; set; }
    }
}
