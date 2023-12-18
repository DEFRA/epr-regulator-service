using System.Diagnostics.CodeAnalysis;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications;

[ExcludeFromCodeCoverage]
public class TransferBannerViewModel
{
    public string OldRegulatorName { get; set; }
    public string NewRegulatorName { get; set; }
    public DateTimeOffset TransferredDate { get; set; }
    public string TransferComments { get; set; }
}
