using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

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

        CreateMap<RegistrationTask, RegistrationTaskViewModel>()
            .ForMember(dest => dest.StatusCssClass, opt => opt.MapFrom(src => MapRegistrationTaskStatusCssClass(src.Status)))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => MapRegistrationTaskStatusText(src.Status)));

        CreateMap<RegistrationMaterial, ApplicationUpdateSession>()
            .ForMember(dest => dest.RegistrationMaterialId, opt => opt.MapFrom(src => src.Id));

        CreateMap<ApplicationUpdateSession, ApplicationGrantedViewModel>();
        CreateMap<ApplicationUpdateSession, ApplicationRefusedViewModel>();
        CreateMap<ApplicationUpdateSession, ApplicationUpdateViewModel>();
    }

    private static string MapRegistrationTaskStatusText(RegulatorTaskStatus taskStatus) =>
        taskStatus switch
        {
            RegulatorTaskStatus.NotStarted => "NOT STARTED YET",
            RegulatorTaskStatus.Completed => "COMPLETE",
            _ => taskStatus.ToString()
        };

    private static string MapRegistrationTaskStatusCssClass(RegulatorTaskStatus taskStatus) =>
        taskStatus switch
        {
            RegulatorTaskStatus.Completed => "govuk-tag--default",
            _ => "govuk-tag--grey"
        };
}