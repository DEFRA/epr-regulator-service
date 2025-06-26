namespace EPR.RegulatorService.Frontend.Core.DTOs.ManageRegistrationSubmissions
{
    using System;
    using System.Text.Json.Serialization;

    using EPR.RegulatorService.Frontend.Core.Enums;

    public class FileDetailsDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FileType Type { get; set; }
        public Guid? FileId { get; set; }
        public string FileName { get; set; }
        public string? BlobName { get; set; }
    }
}
