using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class AuthorisedMaterialsProfile : Profile
{
    public AuthorisedMaterialsProfile()
    {
        CreateMap<RegistrationAuthorisedMaterials, AuthorisedMaterialsViewModel>()
            .ForMember(dest => dest.Materials,
                opt => opt.MapFrom(
                    src => src.MaterialsAuthorisation.OrderByDescending(m => m.IsMaterialRegistered).ThenBy(m => m.MaterialName)));

        CreateMap<MaterialsAuthorisedOnSite, AuthorisedMaterialViewModel>();

        CreateMap<RegistrationAuthorisedMaterials, QueryRegistrationSession>()
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
    }
}