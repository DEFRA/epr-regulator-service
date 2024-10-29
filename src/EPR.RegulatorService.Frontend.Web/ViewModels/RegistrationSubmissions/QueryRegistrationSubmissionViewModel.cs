namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Web.Attributes; 

    public class QueryRegistrationSubmissionViewModel
    {
        public Guid OrganisationId { get; set; }

        [CharacterCount("Error.Query", "Error.QueryTooLong", 400)]
        public string? Query { get; set; }

    }
}
