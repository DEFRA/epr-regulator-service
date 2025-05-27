using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class ManageAccreditationsMappingProfile : Profile
{
    public ManageAccreditationsMappingProfile()
    {
        CreateMap<Registration, ManageAccreditationsViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrganisationName, opt => opt.MapFrom(src => src.OrganisationName))
            .ForMember(dest => dest.SiteAddress, opt => opt.MapFrom(src => src.SiteAddress))
            .ForMember(dest => dest.SiteGridReference, opt => opt.MapFrom(src => src.SiteGridReference))
            .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => src.OrganisationType.ToString()))
            .ForMember(dest => dest.Regulator, opt => opt.MapFrom(src => src.Regulator))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.SiteLevelTasks, opt => opt.MapFrom(src => src.Tasks));

        CreateMap<RegistrationMaterialSummary, AccreditedMaterialViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.MaterialName))
            // Take the single accreditation expected after service filtering
            .ForMember(dest => dest.Accreditation, opt => opt.MapFrom(src => src.Accreditations.SingleOrDefault()));

        CreateMap<Accreditation, AccreditationDetailsViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ApplicationReference, opt => opt.MapFrom(src => src.ApplicationReference))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.DeterminationDate, opt => opt.MapFrom(src => src.DeterminationDate))
            .ForMember(dest => dest.AccreditationYear, opt => opt.MapFrom(src => src.AccreditationYear))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));

        CreateMap<AccreditationTask, AccreditationTaskViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year));

        CreateMap<RegistrationTask, AccreditationTaskViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Year, opt => opt.Ignore());
    }
}
