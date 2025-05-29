using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class SamplingAndInspectionMappingProfile : Profile
{
    public SamplingAndInspectionMappingProfile()
    {
        CreateMap<RegistrationMaterialSamplingPlan, QueryMaterialSession>();
    }
}