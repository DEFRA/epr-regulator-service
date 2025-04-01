using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

using System.Net.Mime;

public class ManageRegistrationsMappingProfile : Profile
{
    public ManageRegistrationsMappingProfile()
    {
        CreateMap<Registration, ManageRegistrationsViewModel>()
            .ForMember(dest => dest.ApplicationOrganisationType,
                       opt => opt.MapFrom(src => src.OrganisationType))
            .ForMember(dest => dest.SiteAddressTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.SiteAddressAndContactDetails)))
            .ForMember(dest => dest.MaterialsAuthorisedOnSiteTask,
            opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.MaterialsAuthorisedOnSite)))
            .ForMember(dest => dest.BusinessAddressTask,
            opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.BusinessAddress)))
            .ForMember(dest => dest.ExporterWasteLicensesTask,
            opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.WasteLicensesPermitsAndExemptions)));

        CreateMap<RegistrationMaterial, ManageRegistrationMaterialViewModel>()
            .ForMember(dest => dest.StatusCssClass,
                opt => opt.MapFrom(src => MapRegistrationMaterialStatusCssClass(src.Status)))
            .ForMember(dest => dest.StatusText,
                opt => opt.MapFrom(src => MapRegistrationMaterialStatusText(src.Status)))
            .ForMember(dest => dest.MaterialWasteLicensesTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.WasteLicensesPermitsAndExemptions)))
            .ForMember(dest => dest.InputsAndOutputsTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.ReprocessingInputsAndOutputs)))
            .ForMember(dest => dest.SamplingAndInspectionPlanTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.SamplingAndInspectionPlan)))
            .ForMember(dest => dest.MaterialDetailsTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.MaterialDetailsAndContact)))
            .ForMember(dest => dest.OverseasReprocessorTask,
                opt => opt.MapFrom(src => src.Tasks.FirstOrDefault(t => t.TaskName == RegulatorTaskType.OverseasReprocessorAndInterimSiteDetails)));

        CreateMap<RegistrationTask, RegistrationTaskViewModel>()
            .ForMember(dest => dest.StatusCssClass, opt => opt.MapFrom(src => MapRegistrationTaskStatusCssClass(src.Status)))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => MapRegistrationTaskStatusText(src.Status)));

        CreateMap<RegistrationMaterial, ApplicationUpdateSession>()
            .ForMember(dest => dest.RegistrationMaterialId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ApplicationUpdateSession, ApplicationGrantedViewModel>()
            .ForMember(dest => dest.Comments, opt => opt.Ignore());
        CreateMap<ApplicationUpdateSession, ApplicationRefusedViewModel>()
            .ForMember(dest => dest.Comments, opt => opt.Ignore()); ;
        CreateMap<ApplicationUpdateSession, ApplicationUpdateViewModel>();
    }

    private static string MapRegistrationMaterialStatusCssClass(ApplicationStatus? status) =>
        status switch
        {
            ApplicationStatus.Granted => "govuk-tag--default",
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
            RegulatorTaskStatus.Completed => "govuk-tag--default",
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