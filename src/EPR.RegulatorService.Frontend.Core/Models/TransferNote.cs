namespace EPR.RegulatorService.Frontend.Core.Models;

public class TransferNote
{
    public int AgencyIndex { get; set; }
    public int NationId { get; set; }
    
    public string? AgencyName { get; set; }
    
    public string? Notes { get; set; }
}