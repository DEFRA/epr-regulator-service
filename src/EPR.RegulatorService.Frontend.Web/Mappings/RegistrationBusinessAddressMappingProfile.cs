using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class RegistrationBusinessAddressMappingProfile : Profile
{
    public RegistrationBusinessAddressMappingProfile()
    {
        CreateMap<RegistrationBusinessAddress, QueryRegistrationSession>()
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
    }
}