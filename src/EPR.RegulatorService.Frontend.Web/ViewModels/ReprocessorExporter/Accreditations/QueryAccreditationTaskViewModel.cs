using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class QueryAccreditationTaskViewModel
{
    public Guid AccreditationId { get; set; }
    public RegulatorTaskType TaskName { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Enter query details")]
    [MaxLength(500, ErrorMessage = "Entry exceeds character maximum")]
    public string Comments { get; set; } = string.Empty;
}
