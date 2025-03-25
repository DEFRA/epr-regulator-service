namespace EPR.RegulatorService.Frontend.Core.Models.FileUpload;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

public class FileUploadRequest
{
    public IFormFile MyFile { set; get; }

    public string FileName { get; set; }

    public SubmissionTypes SubmissionType { get; set; }
    public Guid FileId { get; set; }
    public byte[] FileContent { get; set; }
}

public enum SubmissionTypes
{
    [Display(Name = "pom")]
    Producer = 1,
    [Display(Name = "registration")]
    Registration = 2
}
