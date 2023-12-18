namespace EPR.RegulatorService.Frontend.Core.Models;

public class OrganisationTransferNationRequest
{
    public Guid OrganisationId { get; set; }
    public int TransferNationId { get; set; }
    public string TransferComments { get; set; }
}