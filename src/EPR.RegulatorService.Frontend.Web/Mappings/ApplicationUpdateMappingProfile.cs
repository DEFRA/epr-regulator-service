using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.ApplicationUpdate;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class ApplicationUpdateMappingProfile : Profile
{
    public ApplicationUpdateMappingProfile()
    {
        CreateMap<RegistrationMaterialDetail, ApplicationUpdateSession>()
            .ForMember(dest => dest.RegistrationMaterialId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ApplicationUpdateSession, ApplicationGrantedViewModel>()
            .ForMember(dest => dest.Comments, opt => opt.Ignore());

        CreateMap<ApplicationUpdateSession, ApplicationRefusedViewModel>()
            .ForMember(dest => dest.Comments, opt => opt.Ignore());

        CreateMap<ApplicationUpdateSession, ApplicationUpdateViewModel>();
    }
}