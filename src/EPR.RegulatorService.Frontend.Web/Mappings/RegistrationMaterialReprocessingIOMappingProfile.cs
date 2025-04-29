using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;
namespace EPR.RegulatorService.Frontend.Web.Mappings
{
    public class RegistrationMaterialReprocessingIOMappingProfile : Profile
    {
        public RegistrationMaterialReprocessingIOMappingProfile()
        {
            CreateMap<RegistrationMaterialReprocessingIO, RegistrationMaterialReprocessingIOViewModel>();
        }
    }
}
