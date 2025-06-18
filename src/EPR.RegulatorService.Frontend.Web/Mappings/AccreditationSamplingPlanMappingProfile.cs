using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class AccreditationSamplingPlanMappingProfile : Profile
{
    public AccreditationSamplingPlanMappingProfile()
    {
        CreateMap<AccreditationSamplingPlan, QueryAccreditationSession>();
    }
}