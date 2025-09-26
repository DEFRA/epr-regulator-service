namespace EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Accreditations
{
    using System;

    public class QueryNoteResponseDto
    {
        public String Notes { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
