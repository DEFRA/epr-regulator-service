namespace EPR.RegulatorService.Frontend.UnitTests.Constants;

public static class TransferDetails
{
    public const int SelectedNationIdEngland = 1;
    public const int SelectedNationIdNorthernIreland = 2;
    public const int SelectedNationIdScotland = 3;
    public const int SelectedNationIdWales = 4;
    public const string ScotlandAgencyName = "Scottish Environment Protection Agency (SEPA)";
    public const string NorthernIrelandAgencyName = "Northern Ireland Environment Agency (NIEA)";
    public const string WalesAgencyName = "Natural Resources Wales (NRW)";
    public const string EnglandAgencyName = "England";
    public const string OrganisationName = "Test Organisation";
    public const string TransferDetailsString = "Transferring to you test.";
    public const string LongTransferDetailsString = "This is a test to check that when submitting text which is longer than 200 characters, an error will be thrown and the user will have to reduce the size of this text until it meets the 200 character maximum.";
    public const string ModelErrorValueNoAgencyIndexSelected = "Text.TransferDetails.Error";
    public const string ModelErrorValueNoTransferDetails = "Text.TransferDetailsSummary.Error";
    public const string ModelErrorValueSummaryTooLong = "Text.TransferDetailsSummaryTooLong.Error";
}