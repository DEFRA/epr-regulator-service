namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    public class QueryRegistrationSubmissionViewModel
    {
        public string SubmissionId { get; set; }

        [StringLength(400, ErrorMessage = "Error.QueryTooLong")]
        public string? Query { get; set; }

    }
}
