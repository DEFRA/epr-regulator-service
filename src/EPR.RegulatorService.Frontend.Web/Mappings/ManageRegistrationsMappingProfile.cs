using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

using Core.Sessions.ReprocessorExporter;

public class ManageRegistrationsMappingProfile : Profile
{
    public ManageRegistrationsMappingProfile()
    {
        CreateMap<Registration, ManageRegistrationsViewModel>()
            .ForMember(dest => dest.ApplicationOrganisationType,
                       opt => opt.MapFrom(src => src.OrganisationType));

        CreateMap<RegistrationMaterial, ApplicationUpdateSession>()
            .ForMember(dest => dest.RegistrationMaterialId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ApplicationUpdateSession, ApplicationGrantedViewModel>();
        CreateMap<ApplicationUpdateSession, ApplicationRefusedViewModel>();
        CreateMap<ApplicationUpdateSession, ApplicationUpdateViewModel>();
    }
}