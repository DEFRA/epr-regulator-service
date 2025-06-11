using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

public class QueryAccreditationTaskViewModel
{
    public Guid AccreditationId { get; set; }
    public RegulatorTaskType TaskName { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Comments must be provided")]
    [MaxLength(500, ErrorMessage = "Comments must be 500 characters or less")]
    public string Comments { get; set; } = string.Empty;
}
