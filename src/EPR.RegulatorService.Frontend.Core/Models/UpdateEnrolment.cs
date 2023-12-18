namespace EPR.RegulatorService.Frontend.Core.Models;

public class UpdateEnrolment
{
    public Guid EnrolmentId { get; set; }

    public string EnrolmentStatus { get; set; }

    public string Comments { get; set; }
}