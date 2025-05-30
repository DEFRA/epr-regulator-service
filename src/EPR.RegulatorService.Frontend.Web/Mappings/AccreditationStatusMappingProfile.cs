using AutoMapper;

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
            .ForMember(dest => dest.Year, opt => opt.Ignore());
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
}