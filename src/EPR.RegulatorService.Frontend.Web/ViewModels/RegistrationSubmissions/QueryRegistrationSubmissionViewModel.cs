namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class QueryRegistrationSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }

        [StringLength(400, ErrorMessage = "Error.QueryTooLong")]
        public string? Query { get; set; }

    }
}
