using EPR.RegulatorService.Frontend.Web.ViewModels.Shared.GovUK;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.Web.Extensions;

public static class ModelStateDictionaryExtension
{
    public static List<(string Key, List<ErrorViewModel> Errors)> ToErrorDictionary(this ModelStateDictionary modelState, string?[]? keys = null)
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

        var filterdErrors = new List<ErrorViewModel>();

        filterdErrors = keys != null ? errors.Where(x => keys.Contains(x.Key)).ToList() : errors;

        var errorsDictionary = new List<(string Key, List<ErrorViewModel> Errors)>();

        var groupedErrors = filterdErrors
            .GroupBy(e => e.Key)
            .OrderBy(e => e.Key);

        foreach (var error in groupedErrors)
        {
            errorsDictionary.Add((error.Key, error.ToList()));
        }

        return errorsDictionary;
    }
}