using System;
using System.Collections.Generic;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class ManageAccreditationsViewModel
{
    public Guid Id { get; set; }

    public string OrganisationName { get; set; } = string.Empty;

    public string? SiteAddress { get; set; }

    public string? SiteGridReference { get; set; }

    public string? Regulator { get; set; }

    public string? ApplicationType { get; set; }

    public List<AccreditedMaterialViewModel> Materials { get; set; } = [];

    public List<AccreditationTaskViewModel> SiteLevelTasks { get; set; } = [];
}