using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
using EPR.RegulatorService.Frontend.Web.ViewModels.Shared;

using Microsoft.AspNetCore.Html;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

[ExcludeFromCodeCoverage]
public class RegistrationSubmissionsListViewModel
{
    public IEnumerable<RegistrationSubmissionDetailsViewModel> PagedRegistrationSubmissions { get; set; }

    public PaginationNavigationModel PaginationNavigationModel { get; set; }

    public RegistrationSubmissionsFilterViewModel RegistrationsFilterModel { get; set; } = new RegistrationSubmissionsFilterViewModel();
    public int NationId { get; internal set; }
}

[ExcludeFromCodeCoverage]
//public class DataGrid<T>
//{
//    public IEnumerable<T> Data { get; set; }
//    public List<DataGridHeader> Headers { get; set; } = new List<DataGridHeader>();
//    public PaginationNavigationModel PaginationNavigationModel { get; set; }
//    public string NoResultsMessage { get; set; } = "No results found.";
//    public string AriaLabel { get; set; } = "Data Grid";
//}

//public class DataGridHeader
//{
//    public string DisplayName { get; set; }
//    public string PropertyName { get; set; }
//    public Func<object, Microsoft.AspNetCore.Html.IHtmlContent> Template { get; set; }
//    public string Class { get; set; }
//    public string DataClass { get; set; }
//}

public class DataGrid<T>
{
    public IEnumerable<T> Data { get; set; }
    public List<DataGridHeader<T>> Headers { get; set; } = new List<DataGridHeader<T>>();
    public PaginationNavigationModel PaginationNavigationModel { get; set; }
    public string NoResultsMessage { get; set; } = "No results found.";
    public string AriaLabel { get; set; } = "Data Grid";
}

public class DataGridHeader<T>
{
    public string DisplayName { get; set; }
    public Func<T, object> ValueExpression { get; set; }
    public Func<T, IHtmlContent> Template { get; set; }
    public string Class { get; set; }
    public string DataClass { get; set; }
}