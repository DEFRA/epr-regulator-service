//using System;
//using System.Collections.Generic;

//using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

//namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;

//public class Accreditation
//{
//    public int RegistrationId { get; set; }
//    public string OrganisationName { get; set; } = string.Empty;
//    public string? SiteAddress { get; set; }
//    public string? SiteGridReference { get; set; }
//    public ApplicationOrganisationType OrganisationType { get; set; }
//    public string Regulator { get; set; } = string.Empty;

//    public List<AccreditationTask> SiteLevelTasks { get; set; } = [];
//    public List<AccreditedMaterial> Materials { get; set; } = [];
//}

//public class AccreditedMaterial
//{
//    public int MaterialId { get; set; }
//    public string MaterialName { get; set; } = string.Empty;
//    public AccreditationDetails Accreditation { get; set; } = new();
//}

//public class AccreditationDetails
//{
//    public int AccreditationId { get; set; }
//    public string ApplicationReference { get; set; } = string.Empty;
//    public string Status { get; set; } = string.Empty;
//    public DateTime? DeterminationDate { get; set; }
//    public List<AccreditationTask> Tasks { get; set; } = [];
//}

//public class AccreditationTask
//{
//    public int Id { get; set; }
//    public int TaskId { get; set; }
//    public string TaskName { get; set; } = string.Empty;
//    public string Status { get; set; } = string.Empty;
//    public string? Year { get; set; }
//}
