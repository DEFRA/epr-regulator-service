namespace EPR.RegulatorService.Frontend.UnitTests.Constants;

public static class RejectSubmissionDetails
{
    public const string RejectionReasonString = "Just a test.";
    public const string LongRejectionReasonString = "This is a test to check that when submitting text which is longer than 500 characters, an error will be thrown and the user will have to reduce the size of this text until it meets the 500 character maximum. This is a test to check that when submitting text which is longer than 500 characters, an error will be thrown and the user will have to reduce the size of this text until it meets the 500 character maximum. This is a test to check that when submitting text which is longer than 500 characters, an error will be thrown and the user will have to reduce the size of this text until it meets the 500 character maximum.";
    public const string ModelErrorValueNoResubmissionOptionSelected = "Error.ResubmissionRequired";
    public const string ModelErrorValueNoRejectionReason = "Error.RejectionReason";
    public const string ModelErrorValueRejectionReasonTooLong = "Error.RejectionReasonTooLong";
}