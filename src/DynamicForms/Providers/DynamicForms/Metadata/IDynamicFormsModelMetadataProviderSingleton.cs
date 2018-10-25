using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DynamicForms.Providers.DynamicForms.Metadata
{
    public interface IDynamicFormsModelMetadataProviderSingleton : IModelMetadataProvider
    {
        IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType, ICustomTypeDescriptor model);
    }
}
