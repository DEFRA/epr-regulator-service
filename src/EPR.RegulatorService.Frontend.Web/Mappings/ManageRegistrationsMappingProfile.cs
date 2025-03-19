namespace EPR.RegulatorService.Frontend.Web.Mappings;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

public class ManageRegistrationsMappingProfile : Profile
{
    public ManageRegistrationsMappingProfile()
    {
        CreateMap<Registration, ManageRegistrationsViewModel>()
            .ForMember(dest => dest.ApplicationOrganisationType,
                       opt => opt.MapFrom(src => src.OrganisationType));
    }
}