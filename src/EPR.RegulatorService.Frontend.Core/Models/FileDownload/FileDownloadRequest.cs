namespace EPR.RegulatorService.Frontend.Core.Models.FileDownload;

using System;

using EPR.RegulatorService.Frontend.Core.Enums;

public class FileDownloadRequest
{
    public Guid SubmissionId { get; set; }

    public Guid? FileId { get; set; }

    public string? BlobName { get; set; }

    public string FileName { get; set; }
    public SubmissionType SubmissionType { get; set; }
}
