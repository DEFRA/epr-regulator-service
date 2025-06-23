namespace EPR.RegulatorService.Frontend.Core.Exceptions.ManageRegistrationSubmissions
{
    using System;

    public class RegistrationSubmissionNotFoundException(Guid submissionId) : Exception($"Submission with ID {submissionId} was not found.")
    {
    }
}
