namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels.Shared.Helpers
{
    using System.ComponentModel.DataAnnotations;

    public static class ValidationHelper
    {
        public static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
