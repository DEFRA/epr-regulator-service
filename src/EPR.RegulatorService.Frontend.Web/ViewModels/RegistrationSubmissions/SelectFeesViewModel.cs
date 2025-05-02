namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

using EPR.RegulatorService.Frontend.Web.Attributes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public class SelectFeesViewModel : IValidatableObject
{
    public Guid SubmissionId { get; set; }

    public decimal ApplicationFee { get; set; }

    public string ApplicationFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", ApplicationFee);

    public bool ApplicationFeeSectionEnable => ApplicationFee > 0;

    public bool IsComplianceSchemeChecked { get; set; }

    [ConditionalRequired("IsComplianceSchemeChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedComplianceSchemeAmount { get; set; }

    public bool IsSmallProducerChecked { get; set; }

    public int SmallProducerCount { get; set; }

    public bool SmalProducerSectionEnable => SmallProducerCount > 0 || SmallProducerFee > 0;

    public decimal SmallProducerFee { get; set; }

    public string SmallProducerFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", SmallProducerFee);

    [ConditionalRequired("IsSmallProducerChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedSmallProducerFee { get; set; }

    public bool IsLargeProducerChecked { get; set; }

    public int LargeProducerCount { get; set; }

    public bool LargeProducerSectionEnable => LargeProducerCount > 0 || LargeProducerFee > 0;

    public decimal LargeProducerFee { get; set; }

    public string LargeProducerFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", LargeProducerFee);

    [ConditionalRequired("IsLargeProducerChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedLargeProducerFee { get; set; }

    public bool IsOnineMarketPlaceChecked { get; set; }

    public int OnlineMarketPlaceCount { get; set; }

    public bool OnlineMarketPlaceSectionEnable => OnlineMarketPlaceCount > 0 || OnlineMarketPlaceFee > 0;

    public decimal OnlineMarketPlaceFee { get; set; }

    public string OnlineMarketPlaceFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", OnlineMarketPlaceFee);

    [ConditionalRequired("IsOnineMarketPlaceChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedOnlineMarketPlaceFee { get; set; }

    public bool IsSubsidiariesCompanyChecked { get; set; }

    public int SubsidiariesCompanyCount { get; set; }

    public bool SubsidiariesCompanySectionEnable => SubsidiariesCompanyCount > 0 || SubsidiariesCompanyFee > 0;

    public decimal SubsidiariesCompanyFee { get; set; }

    public string SubsidiariesCompanyFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", SubsidiariesCompanyFee);

    [ConditionalRequired("IsSubsidiariesCompanyChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedSubsidiariesCompanyFee { get; set; }

    public bool IsLateProducerChecked { get; set; }

    public int LateProducerCount { get; set; }

    public bool LateProducerSectionEnable => LateProducerCount > 0 || LateProducerFee > 0;

    public decimal LateProducerFee { get; set; }

    public string LateProducerFeeInPounds => string.Format(CultureInfo.CreateSpecificCulture("en-GB"), "{0:C}", LateProducerFee);

    [ConditionalRequired("IsLateProducerChecked", ErrorMessage = "Enter a waiver amount")]
    [Range(0, double.MaxValue, ErrorMessage = "The value cannot be negative")]
    public decimal WavedLateProducerFee { get; set; }

    public bool IsComplianceSchemeSelected { get; set; }

    public bool IsProducerSelected { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsComplianceSchemeChecked && !IsSmallProducerChecked && !IsLargeProducerChecked &&
            !IsOnineMarketPlaceChecked && !IsSubsidiariesCompanyChecked && !IsLateProducerChecked)
        {
            yield return new ValidationResult("Select a fee to waive", new[] { nameof(IsComplianceSchemeChecked) });
        }

        if (WavedComplianceSchemeAmount > ApplicationFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }

        if (WavedSmallProducerFee > SmallProducerFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }

        if (WavedLargeProducerFee > LargeProducerFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }

        if (WavedOnlineMarketPlaceFee > OnlineMarketPlaceFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }

        if (WavedSubsidiariesCompanyFee > SubsidiariesCompanyFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }

        if (WavedLateProducerFee > LateProducerFee)
        {
            yield return new ValidationResult("Waved amount cannot be more than actual amount", new[] { nameof(WavedComplianceSchemeAmount) });
        }
    }
}