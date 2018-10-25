using DynamicForms.ModelBinding;
using DynamicForms.ViewModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace DynamicForms.Providers.DynamicForms.ModelBinding
{
    public class DynamicFormsModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.ModelType;
            if (modelType.IsAssignableFrom(typeof(DynamicForm)))
            {
                var dynamicFormsPresentationService = (IDynamicFormsService)context.Services.GetService(typeof(IDynamicFormsService));
                return new DynamicFormsModelBinder(context, dynamicFormsPresentationService);
                //return new BinderTypeModelBinder(typeof(DynamicFormsModelBinder));
                //BinderTypeModelBinder acts as a factory for model binders and provides dependency injection (DI). The AuthorEntityBinder requires DI to access EF Core. Use BinderTypeModelBinder if your model binder requires services from DI.
            }

            return null;
        }
    }
}
