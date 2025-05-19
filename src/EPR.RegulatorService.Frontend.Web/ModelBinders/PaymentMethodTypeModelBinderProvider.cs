using System.Diagnostics.CodeAnalysis;

using EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.Web.ModelBinders;

[ExcludeFromCodeCoverage]
public class PaymentMethodTypeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(PaymentMethodType))
        {
            return new PaymentMethodTypeModelBinder();
        }

        return null;
    }
}