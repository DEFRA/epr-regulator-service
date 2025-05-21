using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Accreditations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class ManageAccreditationsMappingProfile : Profile
{
    public ManageAccreditationsMappingProfile()
    {
        CreateMap<Registration, ManageAccreditationsViewModel>()
            .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrganisationName, opt => opt.MapFrom(src => src.OrganisationName))
            .ForMember(dest => dest.SiteAddress, opt => opt.MapFrom(src => src.SiteAddress))
            .ForMember(dest => dest.SiteGridReference, opt => opt.MapFrom(src => src.SiteGridReference))
            .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => src.OrganisationType.ToString()))
            .ForMember(dest => dest.Regulator, opt => opt.MapFrom(src => src.Regulator))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.SiteLevelTasks, opt => opt.MapFrom(src => src.Tasks));

        CreateMap<RegistrationMaterialSummary, AccreditedMaterialViewModel>()
            .ForMember(dest => dest.MaterialId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.MaterialName))
            .ForMember(dest => dest.Accreditation, opt => opt.MapFrom(src => src.Accreditation));

        CreateMap<Accreditation, AccreditationDetailsViewModel>()
            .ForMember(dest => dest.AccreditationId, opt => opt.MapFrom(src => src.AccreditationId))
            .ForMember(dest => dest.ApplicationReference, opt => opt.MapFrom(src => src.ApplicationReference))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.DeterminationDate, opt => opt.MapFrom(src => src.DeterminationDate))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks));

        CreateMap<AccreditationTask, AccreditationTaskViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year));

        CreateMap<RegistrationTask, AccreditationTaskViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
            .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => (int)src.TaskName))
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Year, opt => opt.Ignore());
    }

    private static string MapRegistrationMaterialStatusCssClass(ApplicationStatus? status) =>
        status switch
        {
            ApplicationStatus.Granted => "govuk-tag--green",
            ApplicationStatus.Refused => "govuk-tag--red",
            _ => "govuk-tag--grey"
        };

    private static string MapRegistrationMaterialStatusText(ApplicationStatus? status)
    {
        if (status == null)
        {
            return "Not started yet";
        }

        return status.ToString()!;
    }

    private static string MapRegistrationTaskStatusCssClass(RegulatorTaskStatus taskStatus) =>
        taskStatus switch
        {
            RegulatorTaskStatus.Queried => "govuk-tag--orange",
            RegulatorTaskStatus.Completed => "govuk-tag--blue",
            _ => "govuk-tag--grey"
        };

    private static string MapRegistrationTaskStatusText(RegulatorTaskStatus taskStatus) =>
        taskStatus switch
        {
            RegulatorTaskStatus.NotStarted => "Not started yet",
            RegulatorTaskStatus.Completed => "Reviewed",
            _ => taskStatus.ToString()
        };
}
