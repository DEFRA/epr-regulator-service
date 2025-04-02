namespace EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public enum RegistrationTaskStatus
{
    // TODO: Need to confirm these names match the database values

    NotStarted,
    Started,
    Completed,
    CannotStartYet,
    Queried
}