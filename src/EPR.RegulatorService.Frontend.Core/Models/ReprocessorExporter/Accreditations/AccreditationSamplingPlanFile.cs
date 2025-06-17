namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
public class AccreditationSamplingPlanFile 
{
    public required string Filename { get; set; }
    public required string FileId { get; set; }
    public DateTime? DateUploaded { get; set; }
    public string? UpdatedBy { get; set; }
    public required string FileUploadType { get; set; }
    public required string FileUploadStatus { get; set; }
}