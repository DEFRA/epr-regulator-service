namespace EPR.RegulatorService.Frontend.Core.Services
{
    public interface ISubmissionService
    {
        (int[] Years, string[] Periods) GetFilteredSubmissionYearsAndPeriods();
    }
}
