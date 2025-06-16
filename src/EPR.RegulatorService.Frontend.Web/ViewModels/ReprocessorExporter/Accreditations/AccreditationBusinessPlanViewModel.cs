namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class AccreditationBusinessPlanViewModel
{
    public Guid AccreditationId { get; set; }
    public string OrganisationName { get; set; }
    public string SiteAddress { get; set; }
    public string MaterialName { get; set; }
    public decimal InfrastructurePercentage { get; set; }
    public string InfrastructureNotes { get; set; }
    public decimal RecycledWastePercentage { get; set; }
    public string RecycledWasteNotes { get; set; }
    public decimal BusinessCollectionsPercentage { get; set; }
    public string BusinessCollectionsNotes { get; set; }
    public decimal CommunicationsPercentage { get; set; }
    public string CommunicationsNotes { get; set; }
    public decimal NewMarketsPercentage { get; set; }
    public string NewMarketsNotes { get; set; }
    public decimal NewUsersRecycledPackagingWastePercentage { get; set; }
    public string NewUsersRecycledPackagingWasteNotes { get; set; }
    public decimal NotCoveredOtherCategoriesPercentage { get; set; }
    public string NotCoveredOtherCategoriesNotes { get; set; }
    public string TaskStatus { get; set; }
    public List<QueryNote> QueryNotes { get; set; }
}

