namespace EPR.RegulatorService.Frontend.Web.ViewModels.Registrations
{
    [ExcludeFromDescription]
    public class RegistrationListRequest
    {
        public string? SearchOrganisationName { get; set; }
        public string? SearchOrganisationReference { get; set; }
        public bool IsDirectProducerChecked { get; set; }
        public bool IsComplianceSchemeChecked { get; set; }
        public bool IsPendingRegistrationChecked { get; set; }
        public bool IsAcceptedRegistrationChecked { get; set; }
        public bool IsRejectedRegistrationChecked { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
