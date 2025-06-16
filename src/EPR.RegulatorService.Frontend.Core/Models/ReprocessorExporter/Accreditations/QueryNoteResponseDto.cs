namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class QueryNoteResponseDto
    {
        public String Notes { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
