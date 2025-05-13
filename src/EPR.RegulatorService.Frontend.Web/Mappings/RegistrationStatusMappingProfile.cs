using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class RegistrationStatusMappingProfile : Profile
{
    public RegistrationStatusMappingProfile()
    {
        CreateMap<RegistrationMaterialPaymentFees, RegistrationStatusSession>();

        CreateMap<RegistrationStatusSession, FeesDueViewModel>();
        CreateMap<RegistrationStatusSession, PaymentCheckViewModel>();
        CreateMap<RegistrationStatusSession, PaymentMethodViewModel>();
        CreateMap<RegistrationStatusSession, PaymentDateViewModel>()
           .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Day : (int?)null))
           .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Month : (int?)null))
           .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.PaymentDate.HasValue ? src.PaymentDate.Value.Year : (int?)null));

        CreateMap<RegistrationStatusSession, PaymentReviewViewModel>();

        CreateMap<RegistrationStatusSession, OfflinePaymentRequest>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.FeeAmount))
            .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => src.ApplicationReferenceNumber));
    }
}