namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using EPR.RegulatorService.Frontend.Web.Attributes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public class SelectFeesViewModel : IValidatableObject
{
    public Guid SubmissionId { get; set; }

    public decimal ApplicationFee { get; set; }

    public bool ApplicationFeeSectionEnable => ApplicationFee > 0;

    public bool IsComplianceSchemeChecked { get; set; }

    [ConditionalRequired("IsComplianceSchemeChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedComplianceSchemeAmount { get; set; }

    public bool IsSmallProducerChecked { get; set; }

    public int SmallProducerCount { get; set; }

    public bool SmalProducerSectionEnable => SmallProducerCount > 0 || SmallProducerFee > 0;

    public decimal SmallProducerFee { get; set; }

    [ConditionalRequired("IsSmallProducerChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedSmallProducerFee { get; set; }

    public bool IsLargeProducerChecked { get; set; }

    public int LargeProducerCount { get; set; }

    public bool LargeProducerSectionEnable => LargeProducerCount > 0 || LargeProducerFee > 0;

    public decimal LargeProducerFee { get; set; }

    [ConditionalRequired("IsLargeProducerChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedLargeProducerFee { get; set; }

    public bool IsOnineMarketPlaceChecked { get; set; }

    public int OnlineMarketPlaceCount { get; set; }

    public bool OnlineMarketPlaceSectionEnable => OnlineMarketPlaceCount > 0 || OnlineMarketPlaceFee > 0;

    public decimal OnlineMarketPlaceFee { get; set; }

    [ConditionalRequired("IsOnineMarketPlaceChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedOnlineMarketPlaceFee { get; set; }

    public bool IsSubsidiariesCompanyChecked { get; set; }

    public int SubsidiariesCompanyCount { get; set; }

    public bool SubsidiariesCompanySectionEnable => SubsidiariesCompanyCount > 0 || SubsidiariesCompanyFee > 0;

    public decimal SubsidiariesCompanyFee { get; set; }

    [ConditionalRequired("IsSubsidiariesCompanyChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedSubsidiariesCompanyFee { get; set; }

    public bool IsLateProducerChecked { get; set; }

    public int LateProducerCount { get; set; }

    public bool LateProducerSectionEnable => LateProducerCount > 0 || LateProducerFee > 0;

    public decimal LateProducerFee { get; set; }

    [ConditionalRequired("IsLateProducerChecked", ErrorMessage = "Select a fee to waive")]
    public decimal WavedLateProducerFee { get; set; }

    public bool IsComplianceSchemeSelected { get; set; }

    public bool IsProducerSelected { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsComplianceSchemeChecked && !IsSmallProducerChecked && !IsLargeProducerChecked &&
            !IsOnineMarketPlaceChecked && !IsSubsidiariesCompanyChecked && !IsLateProducerChecked)
        {
            yield return new ValidationResult("Select a fee to waive.", new[] { nameof(IsComplianceSchemeChecked) });
        }
    }
}