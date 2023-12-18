using PhoneNumbers;
using System.Text.RegularExpressions;

namespace EPR.RegulatorService.Frontend.Core.Validations;

public static class TelephoneNumberValidator
{
    public static bool IsValid(string telephoneNumber)
    {
        try
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var phoneNumber = phoneNumberUtil.Parse(telephoneNumber, "GB");

            bool isValidRegexMatch = Regex.IsMatch(telephoneNumber, @"^[+ 0-9()]*$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            bool isValidNumber = phoneNumberUtil.IsValidNumber(phoneNumber);

            return isValidNumber && isValidRegexMatch;
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}