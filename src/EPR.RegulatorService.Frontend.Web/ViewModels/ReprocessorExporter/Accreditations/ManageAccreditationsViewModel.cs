using System;
using System.Collections.Generic;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class ManageAccreditationsViewModel
{
    public int RegistrationId { get; set; }
    public string OrganisationName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? SiteGridReference { get; set; }
    public string? Regulator { get; set; }
    public string? ApplicationType { get; set; } // e.g., Reprocessor/Exporter

    public List<AccreditedMaterialViewModel> Materials { get; set; } = [];
    public List<AccreditationTaskViewModel> SiteLevelTasks { get; set; } = [];
}

public class AccreditedMaterialViewModel
{
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public AccreditationDetailsViewModel Accreditation { get; set; } = new();
}

public class AccreditationDetailsViewModel
{
    public int AccreditationId { get; set; }
    public string ApplicationReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DeterminationDate { get; set; }
    public List<AccreditationTaskViewModel> Tasks { get; set; } = [];
}

public class AccreditationTaskViewModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Year { get; set; }
}
