namespace EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations
{
    public class UkSiteDetailsViewModel
    {
        public required int RegistrationId { get; init; }

        public required string Location { get; init; }

        public required string SiteAddress { get; init; }

        public required string SiteGridReference { get; init; }

        public required string LegalAddress { get; init; }
    }
}
