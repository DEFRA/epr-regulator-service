namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class SelectFeesViewModel
    {
        public Guid SubmissionId { get; set; }

        public decimal ApplicationFee { get; set; }

        public bool ApplicationFeeSectionEnable => ApplicationFee > 0;

        public bool IsComplianceSchemeChecked { get; set; }

        [ConditionalRequired("IsComplianceSchemeChecked", ErrorMessage = "Please enter a waiver amount.")]                
        public decimal WavedComplianceSchemeAmount { get; set; }

        public bool IsSmallProducerChecked { get; set; }

        public int SmallProducerCount { get; set; }

        public bool SmalProducerSectionEnable => SmallProducerCount > 0 || SmallProducerFee > 0;

        public decimal SmallProducerFee { get; set; }

        [ConditionalRequired("IsSmallProducerChecked", ErrorMessage = "Please enter a waiver amount.")]
        public decimal WavedSmallProducerFee { get; set; }

        public int LargeProducerCount { get; set; }

        public bool LargeProducerSectionEnable => LargeProducerCount > 0 || LargeProducerFee > 0;

        public decimal LargeProducerFee { get; set; }

        public decimal WavedLargeProdcuerFee { get; set; }

        public int LateProducerCount { get; set; }

        public bool LateProducerSectionEnable => LateProducerCount > 0 || LateProducerFee > 0;

        public decimal LateProducerFee { get; set; }

        public decimal WavedLateProducerFee { get; set; }

        public int OnlineMarketPlaceCount { get; set; }

        public bool OnlineMarketPlaceSectionEnable => OnlineMarketPlaceCount > 0 || OnlineMarketPlaceFee > 0;

        public decimal OnlineMarketPlaceFee { get; set; }

        public decimal WavedOnlineMarketPlaceFee { get; set; }

        public int SubsidiariesCompanyCount { get; set; }

        public bool SubsidiariesCompanySectionEnable => SubsidiariesCompanyCount > 0 || SubsidiariesCompanyFee > 0;

        public decimal SubsidiariesCompanyFee { get; set; }

        public decimal WavedSubsidiariesCompanyFee { get; set; }

        public bool IsComplianceSchemeSelected { get; set; }

        public bool IsProducerSelected { get; set; }
    }


    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;

        public ConditionalRequiredAttribute(string dependentProperty)
        {
            _dependentProperty = dependentProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dependentProperty);

            if(property == null)
            {
                return new ValidationResult($"Unknown Property: {_dependentProperty}");
            }

            var dependentValue = property.GetValue(validationContext.ObjectInstance);

            if(dependentValue is bool shouldValidate && shouldValidate)
            {
                if(value == null || (value is decimal decimalValue && decimalValue == 0))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }       
    }
}
