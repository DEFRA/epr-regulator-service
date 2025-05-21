namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations
{
    public class AccreditationDetailsViewModel
    {
        public int Id { get; set; }
        public string ApplicationReference { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? DeterminationDate { get; set; }

        public List<AccreditationTaskViewModel> Tasks { get; set; } = [];
    }
}