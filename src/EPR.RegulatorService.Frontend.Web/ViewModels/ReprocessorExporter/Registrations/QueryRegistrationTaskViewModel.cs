namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations
{
    using System.ComponentModel.DataAnnotations;
    using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

    public class QueryRegistrationTaskViewModel
    {
        public Guid RegistrationId { get; set; }
        public RegulatorTaskType TaskName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Comments must be provided")]
        [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
        public string Comments { get; set; } = string.Empty;
    }
}
