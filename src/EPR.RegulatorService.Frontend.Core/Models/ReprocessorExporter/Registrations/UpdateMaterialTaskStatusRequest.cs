using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class UpdateMaterialTaskStatusRequest
    {
        public int TaskName { get; set; }                 
        public int RegistrationMaterialId { get; set; }
        public string Status { get; set; }              
        public string? Comments { get; set; }

    }
}
