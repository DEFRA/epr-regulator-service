namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations
{
    using System.ComponentModel.DataAnnotations;
    using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

    public class QueryMaterialTaskViewModel
    {
        public Guid RegistrationMaterialId { get; set; }
        public RegulatorTaskType TaskName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Enter query details")]
        [MaxLength(500, ErrorMessage = "Entry exceeds character maximum")]
        public string Comments { get; set; } = string.Empty;

    }
}
