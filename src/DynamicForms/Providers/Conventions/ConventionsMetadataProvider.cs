using DynamicForms.Attributes.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace DynamicForms.Providers.Conventions
{ 
    public class ConventionsMetadataProvider : IDisplayMetadataProvider, IValidationMetadataProvider
    {
        private readonly IDisplayConventionFilter[] _metadataFilters;

        public ConventionsMetadataProvider(
            IDisplayConventionFilter[] metadataFilters)
        {
            _metadataFilters = metadataFilters;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            Array.ForEach(_metadataFilters, m => m.TransformMetadata(context));
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
           
        }
    }
}
