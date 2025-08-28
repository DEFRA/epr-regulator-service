namespace EPR.RegulatorService.Frontend.Web.Controllers.RegulatorEnrolment;

public static partial class RegulatorEnrolmentControllerLoggerMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Invited token was not provided in URL for user with id {Id}")]
    public static partial void InvitedTokenWasNotProvided(this ILogger logger, Guid? id);

    [LoggerMessage(Level = LogLevel.Information, Message = "Enrol invited user service for id: {UserId} is successful")]
    public static partial void EnrolmentForInvitedUserSuccessful(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User data in session for id: {UserId} is updated successfully")]
    public static partial void UserDataUpdated(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Enrol invited user service for email: {UserId} failed")]
    public static partial void EnrolmentForInvitedUserFailed(this ILogger logger, Guid userId);
}