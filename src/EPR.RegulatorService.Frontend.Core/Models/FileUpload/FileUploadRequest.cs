namespace EPR.RegulatorService.Frontend.Core.Models.FileUpload;

using Microsoft.AspNetCore.Http;

public class FileUploadRequest
{
    public IFormFile MyFile { set; get; }

    public string FileName { get; set; }
}
