namespace EPR.RegulatorService.Frontend.Web.Controllers.Applications;

public static partial class ApplicationsControllerLoggerMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Organisation id was null.")]
    public static partial void OrganisationIdNull(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error accepting application for: {AcceptedUserEmail} in organisation {OrganisationId}")]
    public static partial void ApplicationAcceptanceError(this ILogger logger, string? acceptedUserEmail, Guid organisationId);
}