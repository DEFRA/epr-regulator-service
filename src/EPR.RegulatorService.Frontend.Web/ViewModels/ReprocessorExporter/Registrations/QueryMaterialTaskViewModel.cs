namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations
{
    using System.ComponentModel.DataAnnotations;

    public class QueryMaterialTaskViewModel
    {
        public int RegistrationMaterialId { get; set; }
        public string TaskName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Comments must be provided")]
        [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
        public string Comments { get; set; } = string.Empty;

    }
}
