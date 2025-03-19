using System.Diagnostics;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Enums;
using EPR.RegulatorService.Frontend.Core.Services.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Configs;
using EPR.RegulatorService.Frontend.Web.Constants;
using EPR.RegulatorService.Frontend.Web.Validations;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Registrations;

[FeatureGate(FeatureFlags.ReprocessorExporter)]
[Route($"{PagePath.ReprocessorExporterRegistrations}/{PagePath.ManageRegistrations}")]
public class ManageRegistrationsController(IRegistrationService registrationService,
    IMapper mapper,
    IValidator<ManageRegistrationsRequest> validator) : Controller
{
    private readonly IRegistrationService _registrationService = registrationService ?? throw new ArgumentNullException(nameof(registrationService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IValidator<ManageRegistrationsRequest> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Index([FromQuery] int id)
    {
        var validationResult = _validator.Validate(new ManageRegistrationsRequest { Id = id });

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var registration = _registrationService.GetRegistrationById(id);

        ViewBag.BackLinkToDisplay = "";

        var model = _mapper.Map<ManageRegistrationsViewModel>(registration);

        return View("~/Views/ReprocessorExporter/Registrations/ManageRegistrations.cshtml", model);
    }
}