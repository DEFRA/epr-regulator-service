using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class SiteDetailsMappingProfile : Profile
{
    public SiteDetailsMappingProfile()
    {
        CreateMap<SiteDetails, SiteDetailsViewModel>()
            .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.Id));
    }
}
