using System.ComponentModel.DataAnnotations;
using EPR.RegulatorService.Frontend.Web.Attributes;
using EPR.RegulatorService.Frontend.Core.Models;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications
{
    public class TransferApplicationViewModel
    {
        public List<RegulatorOrganisation> RegulatorOrganisations { get; set; } = new();

        public int? UserNation { get; set; }
        public string? OrganisationName { get; set; }
        
        [Required(ErrorMessage = "Text.TransferDetails.Error")]
        public int? AgencyIndex { get; set; }
        
        [TransferNationDetails(nameof(AgencyIndex), "Text.TransferDetailsSummary.Error", "Text.TransferDetailsSummaryTooLong.Error", 200)]
        public List<TransferNote> TransferNotes { get; set; }
    }
}