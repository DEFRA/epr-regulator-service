using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class RegistrationWasteLicencesMappingProfile : Profile
{
    public RegistrationWasteLicencesMappingProfile()
    {
        CreateMap<RegistrationWasteLicences, QueryRegistrationSession>()
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
    }
}