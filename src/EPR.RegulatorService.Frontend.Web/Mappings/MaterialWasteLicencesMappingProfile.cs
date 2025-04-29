using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.Constants.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class MaterialWasteLicencesMappingProfile : Profile
{
    public MaterialWasteLicencesMappingProfile()
    {
        CreateMap<RegistrationMaterialWasteLicence, MaterialWasteLicencesViewModel>()
            .ForMember(dest => dest.ReferenceNumberLabel, opt => opt.MapFrom(src => GetReferenceNumberLabel(src.PermitType)))
            .ForMember(dest => dest.CapacityPeriod, opt => opt.MapFrom(src => GetPeriodText(src.CapacityPeriod)))
            .ForMember(dest => dest.MaximumReprocessingPeriod, opt => opt.MapFrom(src => GetPeriodText(src.MaximumReprocessingPeriod)));
    }

    private static string GetReferenceNumberLabel(string permitType) =>
        permitType switch
        {
            PermitTypes.WasteExemption => "Exemption reference(s)",
            PermitTypes.PollutionPreventionAndControlPermit => "PPC permit number",
            PermitTypes.WasteManagementLicence => "Waste management number",
            PermitTypes.InstallationPermit => "Installation permit number",
            PermitTypes.EnvironmentalPermitOrWasteManagementLicence => "Environment permit or waste management number",
            _ => throw new ArgumentOutOfRangeException($"Unexpected permit type: {permitType}")
        };

    private static string GetPeriodText(string? period)
    {
        if (period == null)
        {
            return string.Empty;
        }

        return period switch
        {
            PeriodTypes.PerYear => "Annually",
            PeriodTypes.PerMonth => "Monthly",
            PeriodTypes.PerWeek => "Weekly",
            _ => throw new ArgumentOutOfRangeException($"Unexpected period type: {period}")
        };
    }
}