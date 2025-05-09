namespace EPR.RegulatorService.Frontend.Core.Services
{
    public interface ISubmissionFilterConfigService
    {
        (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods();
    }
}
