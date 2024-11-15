using EPR.RegulatorService.Frontend.Web.ViewModels.Shared.GovUK;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.Web.Extensions;

public static class ModelStateDictionaryExtension
{
    public static List<(string Key, List<ErrorViewModel> Errors)> ToErrorDictionary(this ModelStateDictionary modelState)
    {
        var errors = new List<ErrorViewModel>();

        foreach (var item in modelState)
        {
            foreach (var error in item.Value.Errors)
            {
                errors.Add(new ErrorViewModel
                {
                    Key = item.Key,
                    Message = error.ErrorMessage
                });
            }
        }

        var errorsDictionary = new List<(string Key, List<ErrorViewModel> Errors)>();

        var groupedErrors = errors
            .GroupBy(e => e.Key)
            .OrderBy(e => e.Key);

        foreach (var error in groupedErrors)
        {
            errorsDictionary.Add((error.Key, error.ToList()));
        }

        return errorsDictionary;
    }

    public static List<(string Key, List<ErrorViewModel> Errors)> ToErrorDictionaryByKey(this ModelStateDictionary modelState, string key)
    {
        var errors = new List<ErrorViewModel>();

        foreach (var item in modelState)
        {
            if (item.Key == key)
            {
                foreach (var error in item.Value.Errors)
                {
                    errors.Add(new ErrorViewModel
                    {
                        Key = item.Key,
                        Message = error.ErrorMessage
                    });
                }
            }
        }

        var errorsDictionary = new List<(string Key, List<ErrorViewModel> Errors)>();

        var groupedErrors = errors
            .GroupBy(e => e.Key)
            .OrderBy(e => e.Key);

        foreach (var error in groupedErrors)
        {
            errorsDictionary.Add((error.Key, error.ToList()));
        }

        return errorsDictionary;
    }
}