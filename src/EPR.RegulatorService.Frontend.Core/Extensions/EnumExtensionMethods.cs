namespace EPR.RegulatorService.Frontend.Core.Extensions;

public static class EnumExtensionMethods
{
    public static string GetDescription(this Enum genericEnum)
    {
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
}
