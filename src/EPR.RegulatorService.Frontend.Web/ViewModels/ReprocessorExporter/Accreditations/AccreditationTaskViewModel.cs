namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations
{
    public class AccreditationTaskViewModel
    {
        public Guid? Id { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? Year { get; set; }
    }
}