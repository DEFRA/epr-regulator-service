using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class UpdateMaterialTaskStatusRequest
    {
        public string TaskName { get; set; }                 
        public Guid RegistrationMaterialId { get; set; }
        public string Status { get; set; }              
        public string? Comments { get; set; }

    }
}
