namespace EPR.RegulatorService.Frontend.Web.Controllers.Attributes;

using Constants;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class JourneyAccessAttribute : Attribute
{
    public JourneyAccessAttribute(string pagePath, string journeyType = JourneyName.Applications)
    {
        PagePath = pagePath;
        JourneyType = journeyType;
    }

    public string PagePath { get; }
    public string JourneyType { get; }
}
