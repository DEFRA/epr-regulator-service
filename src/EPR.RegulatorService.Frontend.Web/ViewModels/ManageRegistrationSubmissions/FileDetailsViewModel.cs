namespace EPR.RegulatorService.Frontend.Web.ViewModels.ManageRegistrationSubmissions
{
    using EPR.RegulatorService.Frontend.Core.Enums;

    public class FileDetailsViewModel
    {
        public FileType Type { get; set; }
        public string DownloadType { get; set; }
        public string Label { get; set; }
        public string FileName { get; set; }
        public Guid? FileId { get; set; }
        public string? BlobName { get; set; }
    }
}
