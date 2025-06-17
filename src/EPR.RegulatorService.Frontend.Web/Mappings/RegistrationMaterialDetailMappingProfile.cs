using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class RegistrationMaterialDetailMappingProfile : Profile
{
    public RegistrationMaterialDetailMappingProfile()
    {
        CreateMap<RegistrationMaterialDetail, QueryMaterialSession>()
            .ForMember(dest => dest.RegistrationMaterialId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
    }
}