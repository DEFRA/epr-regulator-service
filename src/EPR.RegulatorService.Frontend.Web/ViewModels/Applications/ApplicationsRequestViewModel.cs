namespace EPR.RegulatorService.Frontend.Web.ViewModels.Applications
{
    using System.Diagnostics.CodeAnalysis;

    using EPR.RegulatorService.Frontend.Core.Models;

    [ExcludeFromCodeCoverage]
    public class ApplicationsRequestViewModel
    {
        public string? SearchOrganisationName { get; set; }
        public bool IsApprovedUserTypeChecked { get; set; }
        public bool IsDelegatedUserTypeChecked { get; set; }
        public EndpointResponseStatus TransferNationResult { get; set; }
        public string? TransferredOrganisationName { get; set; }
        public string? TransferredOrganisationAgency { get; set; }
        public bool ClearFilters { get; set; } = false;
        public bool IsFilteredSearch { get; set; } = false;
        public int? PageNumber { get; set; } = null;
    }

}
