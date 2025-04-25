using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class AuthorisedMaterialsProfile : Profile
{
    public AuthorisedMaterialsProfile()
    {
        CreateMap<Registration, AuthorisedMaterialsViewModel>()
            .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Materials,
                opt => opt.MapFrom(
                    src => src.Materials.OrderByDescending(m => m.IsMaterialRegistered).ThenBy(m => m.MaterialName)));

        CreateMap<RegistrationMaterialSummary, AuthorisedMaterialViewModel>();
    }
}