namespace EPR.RegulatorService.Frontend.Core.Services
{
    public interface ISubmissionPeriodService
    {
        (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods();
    }
}
