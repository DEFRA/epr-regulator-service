using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations
{
    public class UpdateRegistrationTaskStatusRequest
    {
        public string TaskName { get; set; }
        public int RegistrationId { get; set; }
        public string Status { get; set; }
        public string? Comments { get; set; }

    }
}
