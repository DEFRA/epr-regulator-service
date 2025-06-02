using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.Query;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class QueryNoteMappingProfile : Profile
{
    public QueryNoteMappingProfile()
    {
        CreateMap<RegistrationStatusSession, QueryMaterialSession>()
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
        CreateMap<QueryMaterialSession, AddQueryNoteViewModel>()
            .ForMember(dest => dest.Note, opt => opt.Ignore())
            .ForMember(dest => dest.FormAction, opt => opt.Ignore());
        CreateMap<QueryRegistrationSession, AddQueryNoteViewModel>()
            .ForMember(dest => dest.Note, opt => opt.Ignore())
            .ForMember(dest => dest.FormAction, opt => opt.Ignore());
    }
}