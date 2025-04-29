using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class UKSiteDetailsMappingProfile : Profile
{
    public UKSiteDetailsMappingProfile()
    {
        CreateMap<UkSiteDetails, UkSiteDetailsViewModel>()
            .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.Id));
    }
}
