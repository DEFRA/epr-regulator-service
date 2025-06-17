using AutoMapper;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class WasteCarrierMappingProfile : Profile
{
    public WasteCarrierMappingProfile()
    {
        CreateMap<WasteCarrierDetails, WasteCarrierDetailsViewModel>();

        CreateMap<WasteCarrierDetails, QueryRegistrationSession>()
            .ForMember(dest => dest.PagePath, opt => opt.Ignore());
    }
}