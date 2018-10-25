using DynamicForms.Attributes.Common;
using DynamicForms.Providers.Attributes;
using DynamicForms.Providers.Conventions;
using DynamicForms.Providers.DynamicForms.Metadata;
using DynamicForms.Providers.DynamicForms.ModelBinding;
using DynamicForms.Providers.DynamicForms.Validation;
using DynamicForms.Providers.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DynamicForms
{
    public static class DynamicFormsServiceCollectionExtensions
    {
        public static IServiceCollection AddMvcDisplayConventions(this IServiceCollection services, IDisplayConventionFilter[] displayConventions)
        {
            return services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new ConventionsMetadataProvider(displayConventions));
            });
        }

        public static IServiceCollection AddMvcDisplayAttributes(this IServiceCollection services)
        {
            return services.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new AttributeMetadataProvider());
            });
        }

        public static IServiceCollection AddInheritanceValidationAttributeAdapterProvider(this IServiceCollection services)
        {
            services.RemoveAll<IValidationAttributeAdapterProvider>();
            services.AddSingleton<IValidationAttributeAdapterProvider, InheritanceValidationAttributeAdapterProvider>();

            return services;
        }

        public static IServiceCollection AddDynamicForms(this IServiceCollection services)
        {
            services.AddDynamicFormsModelMetadataProvider();
            services.AddDynamicFormsObjectValidator();
            services.AddDynamicFormsAutomaticModelBinding();

            return services;
        }

        public static IServiceCollection AddDynamicFormsModelMetadataProvider(this IServiceCollection services)
        {
            services.RemoveAll<IModelMetadataProvider>();
            return services.AddSingleton<IModelMetadataProvider, DynamicFormsModelMetadataProviderSingleton>();
        }

        public static IServiceCollection AddDynamicFormsObjectValidator(this IServiceCollection services)
        {
            services.RemoveAll<IObjectModelValidator>();
            services.AddSingleton<IObjectModelValidator>(s =>
            {
                var options = s.GetRequiredService<IOptions<MvcOptions>>().Value;
                var metadataProvider = s.GetRequiredService<IDynamicFormsModelMetadataProviderSingleton>();
                return new DynamicFormsObjectValidator(metadataProvider, options.ModelValidatorProviders);
            });

            return services;
        }

        public static IServiceCollection AddDynamicFormsAutomaticModelBinding(this IServiceCollection services)
        {
            return services.Configure<MvcOptions>(options =>
            {
                options.ModelBinderProviders.Insert(0, new DynamicFormsModelBinderProvider());
            });
        }

    }
}
