using System.Diagnostics.CodeAnalysis;


using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.Web.ModelBinders;

[ExcludeFromCodeCoverage]
public class PaymentMethodTypeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        string? value = valueProviderResult.FirstValue;

        var paymentMethod = PaymentMethodType.AllTypes.SingleOrDefault(pm => pm.ToString() == value);

        if (paymentMethod == null)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid payment method.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(paymentMethod);
        return Task.CompletedTask;
    }
}