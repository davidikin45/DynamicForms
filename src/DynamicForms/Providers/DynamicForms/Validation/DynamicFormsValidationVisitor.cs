using DynamicForms.Providers.DynamicForms.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Runtime.CompilerServices;

namespace DynamicForms.Providers.DynamicForms.Validation
{
    public class DynamicFormsValidationVisitor : ValidationVisitor
    {
        public DynamicFormsValidationVisitor(ActionContext actionContext, IModelValidatorProvider validatorProvider, ValidatorCache validatorCache, IDynamicFormsModelMetadataProviderSingleton metadataProvider, ValidationStateDictionary validationState)
            :base(actionContext, validatorProvider, validatorCache, metadataProvider, validationState)
        { }

        protected override bool Visit(ModelMetadata metadata, string key, object model)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();

            if (model != null && !CurrentPath.Push(model))
            {
                // This is a cycle, bail.
                return true;
            }

            var entry = GetValidationEntry(model);
            key = entry?.Key ?? key ?? string.Empty;
            metadata = entry?.Metadata ?? metadata;
            var strategy = entry?.Strategy;

            if (ModelState.HasReachedMaxErrors)
            {
                SuppressValidation(key);
                return false;
            }
            else if (entry != null && entry.SuppressValidation)
            {
                // Use the key on the entry, because we might not have entries in model state.
                SuppressValidation(entry.Key);
                CurrentPath.Pop(model);
                return true;
            }

            using (StateManager.Recurse(this, key ?? string.Empty, metadata, model, strategy))
            {
                if (Metadata.IsEnumerableType)
                {
                    return VisitComplexType(DynamicFormsCollectionValidationStrategy.Instance);
                }

                if (Metadata.IsComplexType)
                {
                    return VisitComplexType(DynamicFormsComplexObjectValidationStrategy.Instance);
                }

                return VisitSimpleType();
            }
        }
    }
}
