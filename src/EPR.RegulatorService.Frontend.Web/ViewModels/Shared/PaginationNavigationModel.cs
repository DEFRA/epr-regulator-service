namespace EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

public class PaginationNavigationModel
{
    public int CurrentPage { get; set; }
    
    public int PageCount { get; set; }
    
    public string ActionName { get; set; }
    
    public string ControllerName { get; set; }
}