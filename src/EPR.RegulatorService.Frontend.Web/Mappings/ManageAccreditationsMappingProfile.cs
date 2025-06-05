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
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.OrganisationName, opt => opt.MapFrom(src => src.OrganisationName))
            .ForMember(dest => dest.SiteAddress, opt => opt.MapFrom(src => src.SiteAddress))
            .ForMember(dest => dest.SiteGridReference, opt => opt.MapFrom(src => src.SiteGridReference))
            .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => src.OrganisationType.ToString()))
            .ForMember(dest => dest.Regulator, opt => opt.MapFrom(src => src.Regulator))
            .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => src.Materials))
            .ForMember(dest => dest.SiteLevelTasks, opt => opt.MapFrom(src =>
                src.Tasks
                   .Where(t => t.TaskName == RegulatorTaskType.AssignOfficer)
                   .Select(MapRegistrationTaskToAccreditationTaskViewModel)
                   .ToList()
            ));

        CreateMap<RegistrationMaterialSummary, AccreditedMaterialViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MaterialName, opt => opt.MapFrom(src => src.MaterialName))
            .ForMember(dest => dest.RegistrationStatusTask, opt => opt.MapFrom(src => MapApplicationStatusToViewModel(src.Status)))
            .ForMember(dest => dest.RegistrationStatusRaw, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Accreditation, opt => opt.MapFrom(src => src.Accreditations.SingleOrDefault()));

        CreateMap<Accreditation, AccreditationDetailsViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ApplicationReference, opt => opt.MapFrom(src => src.ApplicationReference))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusStringToEnum(src.Status)))
            .ForMember(dest => dest.DeterminationDate, opt => opt.MapFrom(src => src.DeterminationDate))
            .ForMember(dest => dest.AccreditationYear, opt => opt.MapFrom(src => src.AccreditationYear))
            .ForMember(dest => dest.PRNTonnageTask, opt => opt.Ignore())
            .ForMember(dest => dest.BusinessPlanTask, opt => opt.Ignore())
            .ForMember(dest => dest.SamplingAndInspectionPlanTask, opt => opt.Ignore())
            .ForMember(dest => dest.CheckAccreditationStatusTask, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.PRNTonnageTask = MapAccreditationTask(src.Tasks, RegulatorTaskType.PRNTonnage);
                dest.BusinessPlanTask = MapAccreditationTask(src.Tasks, RegulatorTaskType.BusinessPlan);
                dest.SamplingAndInspectionPlanTask = MapAccreditationTask(src.Tasks, RegulatorTaskType.SamplingAndInspectionPlan);
                dest.CheckAccreditationStatusTask = MapAccreditationTask(src.Tasks, RegulatorTaskType.DulyMade);
            });


        CreateMap<RegistrationTask, AccreditationTaskViewModel>()
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => MapTaskStatusText(src.Status.ToString(), src.TaskName)))
            .ForMember(dest => dest.StatusCssClass, opt => opt.MapFrom(src => MapTaskStatusCssClass(src.Status.ToString())));
    }

    // Converts a string to ApplicationStatus enum
    private static ApplicationStatus MapStatusStringToEnum(string status)
    {
        if (Enum.TryParse<ApplicationStatus>(status, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return ApplicationStatus.Started; // fallback
    }

    private static AccreditationTaskViewModel MapAccreditationTask(IEnumerable<AccreditationTask> tasks, RegulatorTaskType taskName)
    {
        var match = tasks.FirstOrDefault(t => t.TaskName == taskName.ToString());
        var status = match?.Status ?? string.Empty;

        return new AccreditationTaskViewModel
        {
            TaskName = taskName,
            StatusText = MapTaskStatusText(status, taskName),
            StatusCssClass = MapTaskStatusCssClass(status)
        };
    }

    private static AccreditationTaskViewModel MapRegistrationTaskToAccreditationTaskViewModel(RegistrationTask task)
    {
        return new AccreditationTaskViewModel
        {
            TaskName = task.TaskName,
            StatusText = MapTaskStatusText(task.Status.ToString(), task.TaskName),
            StatusCssClass = MapTaskStatusCssClass(task.Status.ToString())
        };
    }

    private static string MapTaskStatusText(string status) =>
        status.ToLowerInvariant() switch
        {
            "not started" => "Not Started yet",
            "not started yet" => "Not started yet",
            "approved" => "Approved",
            "queried" => "Queried",
            "completed" => "Completed",
            "duly made" => "Duly Made",
            "dulymade" => "Duly Made",
            _ => "Not Started Yet"
        };

    private static string MapTaskStatusText(string status, RegulatorTaskType taskName) =>
        status.ToLowerInvariant() switch
        {
            "completed" => taskName switch
            {
                RegulatorTaskType.CheckRegistrationStatus => "Duly Made",
                RegulatorTaskType.DulyMade => "Duly Made",
                RegulatorTaskType.AssignOfficer => "Officer Assigned",
                _ => "Reviewed"
            },
            "approved" => "Approved",
            "queried" => "Queried",
            _ => "Not started yet"
        };

    private static string MapTaskStatusCssClass(string status) =>
        status.ToLowerInvariant() switch
        {
            "completed" => "govuk-tag--blue",
            "approved" => "govuk-tag--green",
            "queried" => "govuk-tag--orange",
            "duly made" => "govuk-tag--blue",
            "dulymade" => "govuk-tag--blue",
            _ => "govuk-tag--grey"
        };

    private static RegistrationTaskViewModel MapApplicationStatusToViewModel(ApplicationStatus? status) =>
        new()
        {
            TaskName = RegulatorTaskType.CheckRegistrationStatus,
            StatusText = MapApplicationStatusText(status),
            StatusCssClass = MapApplicationStatusCssClass(status)
        };

    private static string MapApplicationStatusText(ApplicationStatus? status) =>
        status switch
        {
            ApplicationStatus.Granted => "Granted",
            ApplicationStatus.Refused => "Refused",
            ApplicationStatus.Withdrawn => "Withdrawn",
            ApplicationStatus.RegulatorReviewing => "Regulator Reviewing",
            _ => "Not started yet"
        };

    private static string MapApplicationStatusCssClass(ApplicationStatus? status) =>
        status switch
        {
            ApplicationStatus.Granted => "govuk-tag--green",
            ApplicationStatus.Refused => "govuk-tag--red",
            ApplicationStatus.Withdrawn => "govuk-tag--grey",
            ApplicationStatus.RegulatorReviewing => "govuk-tag--blue",
            _ => "govuk-tag--grey"
        };
}
