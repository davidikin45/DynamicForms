using DynamicForms.Providers.DynamicForms.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace DynamicForms.Providers.DynamicForms.Validation
{
    //.Net Core Object validator
    public class DynamicFormsObjectValidator : DefaultObjectValidator
    {
        private readonly IDynamicFormsModelMetadataProviderSingleton _customModelMetadataProvider;
        public DynamicFormsObjectValidator(IDynamicFormsModelMetadataProviderSingleton modelMetadataProvider, IList<IModelValidatorProvider> validatorProviders)
            :base(modelMetadataProvider, validatorProviders)
        {
            _customModelMetadataProvider = modelMetadataProvider;
        }

        public override ValidationVisitor GetValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider, ValidatorCache validatorCache, IModelMetadataProvider metadataProvider, ValidationStateDictionary validationState)
        {
            return new DynamicFormsValidationVisitor(
                actionContext,
                validatorProvider,
                validatorCache,
                _customModelMetadataProvider,
                validationState);
        }
    }
}
