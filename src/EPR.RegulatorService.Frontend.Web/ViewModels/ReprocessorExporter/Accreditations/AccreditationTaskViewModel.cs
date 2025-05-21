namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations
{
    public class AccreditationTaskViewModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Year { get; set; }
    }
}