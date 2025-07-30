namespace EPR.RegulatorService.Frontend.Core.Configs;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RegistrationSubmissionsConfig
{
    public const string ConfigSection = "BehaviourManagement:RegistrationSubmissions";

    public bool Show2026RelevantYearFilter { get; set; }

    public int LateFeeCutOffMonth { get; set; }

    public int LateFeeCutOffDay { get; set; }

    public int LateFeeCutOffMonth_2025 { get; set; }

    public int LateFeeCutOffDay_2025 { get; set; }

    public int LateFeeCutOffMonth_CS { get; set; }

    public int LateFeeCutOffDay_CS { get; set; }

    public int LateFeeCutOffMonth_SP { get; set; }

    public int LateFeeCutOffDay_SP { get; set; }

    public int LateFeeCutOffMonth_LP { get; set; }

    public int LateFeeCutOffDay_LP { get; set; }
}