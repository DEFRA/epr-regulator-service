global using System.Net;
global using System.Text;
global using System.Text.Json;

global using EPR.RegulatorService.Frontend.Core.Configs;
global using EPR.RegulatorService.Frontend.Core.Enums;
global using EPR.RegulatorService.Frontend.Core.MockedData;
global using EPR.RegulatorService.Frontend.Core.Models;
global using EPR.RegulatorService.Frontend.Core.Models.CompanyDetails;
global using EPR.RegulatorService.Frontend.Core.Models.FileDownload;
global using EPR.RegulatorService.Frontend.Core.Models.Pagination;
global using EPR.RegulatorService.Frontend.Core.Models.Registrations;
global using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
global using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;
global using EPR.RegulatorService.Frontend.Core.Models.Submissions;

global using Microsoft.AspNetCore.Mvc;

global using EPR.RegulatorService.Frontend.Core.Services;

global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Identity.Web;

global using Moq.Protected;