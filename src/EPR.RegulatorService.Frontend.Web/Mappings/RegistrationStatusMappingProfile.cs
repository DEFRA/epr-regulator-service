using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations.RegistrationStatus;

namespace EPR.RegulatorService.Frontend.Web.Mappings;

public class RegistrationStatusMappingProfile : Profile
{
    public RegistrationStatusMappingProfile()
    {
        CreateMap<RegistrationMaterialPaymentFees, RegistrationStatusSession>();

        CreateMap<RegistrationStatusSession, FeesDueViewModel>();

        CreateMap<RegistrationStatusSession, PaymentCheckViewModel>();

        CreateMap<RegistrationStatusSession, PaymentMethodViewModel>();
    }
}