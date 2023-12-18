namespace EPR.RegulatorService.Frontend.Core.Models;

public class Enrolment
{
    public string ServiceRole { get; set; }
    public string EnrolmentStatus { get; set; }
    public Guid ExternalId { get; set; }
}