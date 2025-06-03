using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations.AccreditationStatus;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class AccreditationStatusMappingProfile : Profile
{
    public AccreditationStatusMappingProfile()
    {
        CreateMap<AccreditationMaterialPaymentFees, AccreditationStatusSession>()
            .ForMember(dest => dest.FullPaymentMade, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
            .ForMember(dest => dest.RegistrationId, opt => opt.Ignore())
            .ForMember(dest => dest.Year, opt => opt.Ignore())
            .ForMember(dest => dest.PrnTonnage, opt => opt.MapFrom(src => MapPrnTonnageType(src.PrnTonnage)));
        CreateMap<AccreditationStatusSession, FeesDueViewModel>();
        CreateMap<AccreditationStatusSession, PaymentCheckViewModel>();
        CreateMap<AccreditationStatusSession, PaymentMethodViewModel>()
            .ForMember(dest => dest.PaymentMethods, opt => opt.Ignore());
        CreateMap<AccreditationStatusSession, PaymentDateViewModel>()
           .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Day : (int?)null))
           .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Month : (int?)null))
           .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Year : (int?)null));
        CreateMap<AccreditationStatusSession, PaymentReviewViewModel>()
            .ForMember(dest => dest.DeterminationDate, opt => opt.Ignore())
            .ForMember(dest => dest.DeterminationWeeks, opt => opt.Ignore()); 
        CreateMap<AccreditationStatusSession, AccreditationOfflinePaymentRequest>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.FeeAmount))
            .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => src.ApplicationReferenceNumber));
    }

    private static string MapPrnTonnageType(PrnTonnageType prnTonnageType) =>
        prnTonnageType switch
        {
            PrnTonnageType.Upto500Tonnes => "Up to 500 tonnes",
            PrnTonnageType.Upto5000Tonnes => "Up to 5,000 tonnes",
            PrnTonnageType.Upto10000Tonnes => "Up to 10,000 tonnes",
            PrnTonnageType.Over10000Tonnes => "Over 10,000 tonnes",
            _ => ""
        };
}