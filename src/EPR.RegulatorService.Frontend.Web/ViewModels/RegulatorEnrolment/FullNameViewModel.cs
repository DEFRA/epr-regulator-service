namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegulatorEnrolment
{
    using System.ComponentModel.DataAnnotations;

    public class FullNameViewModel
    {
        [Required(ErrorMessage = "FirstName.RequiredError")]
        [StringLength(50, ErrorMessage = "FirstName.LengthError")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName.RequiredError")]
        [StringLength(50, ErrorMessage = "LastName.LengthError")]
        public string LastName { get; set; }

        public string? Token { get; set; }
    }
}
