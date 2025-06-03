namespace EPR.RegulatorService.Frontend.Core.Extensions;

using EPR.RegulatorService.Frontend.Core.Enums;

public static class EnumExtensionMethods
{
    public static string GetDescription(this Enum genericEnum)
    {
        if (genericEnum is null)
        {
            return string.Empty;
        }

        var genericEnumType = genericEnum.GetType();
        var memberInfo = genericEnumType.GetMember(genericEnum.ToString());
        if ((memberInfo.Length > 0))
        {
            var attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if ((attribs.Length != 0))
            {
                return ((System.ComponentModel.DescriptionAttribute)attribs[0]).Description;
            }
        }

        return genericEnum.ToString();
    }

    public static RegistrationSubmissionType GetRegistrationSubmissionType(this RegistrationSubmissionOrganisationType registrationSubmissionOrganisationType)
    {
        if (registrationSubmissionOrganisationType is RegistrationSubmissionOrganisationType.compliance)
        {
            return RegistrationSubmissionType.ComplianceScheme;
        }

        else if (registrationSubmissionOrganisationType is
            RegistrationSubmissionOrganisationType.large or RegistrationSubmissionOrganisationType.small)
        {
            return RegistrationSubmissionType.Producer;
        }

        return RegistrationSubmissionType.NotSet;
    }
}
