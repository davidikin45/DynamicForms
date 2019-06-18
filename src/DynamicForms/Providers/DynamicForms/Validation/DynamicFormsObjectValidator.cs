using DynamicForms.Providers.DynamicForms.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace DynamicForms.Providers.DynamicForms.Validation
{
    //.Net Core Object validator
    public class DynamicFormsObjectValidator : ObjectModelValidator
    {
        private readonly MvcOptions _mvcOptions;
        private readonly IDynamicFormsModelMetadataProviderSingleton _customModelMetadataProvider;
        public DynamicFormsObjectValidator(
            IDynamicFormsModelMetadataProviderSingleton modelMetadataProvider, 
            MvcOptions mvcOptions)
            :base(modelMetadataProvider, mvcOptions.ModelValidatorProviders)
        {
            _mvcOptions = mvcOptions;
            _customModelMetadataProvider = modelMetadataProvider;
        }

        public override ValidationVisitor GetValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider, ValidatorCache validatorCache, IModelMetadataProvider metadataProvider, ValidationStateDictionary validationState)
        {
            var visitor = new DynamicFormsValidationVisitor(
                actionContext,
                validatorProvider,
                validatorCache,
                _customModelMetadataProvider,
                validationState)
            {
                MaxValidationDepth = _mvcOptions.MaxValidationDepth,
                ValidateComplexTypesIfChildValidationFails = false
            };

            return visitor;
        }
    }
}
