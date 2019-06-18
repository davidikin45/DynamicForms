using DynamicForms.Providers.DynamicForms.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DynamicForms
{
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ViewDataDictionary.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ModelExplorer.cs
    public static class ModelExplorerExtensions
    {
        public static IEnumerable<ModelExplorer> Contextualize(this ModelExplorer modelExplorer, IModelMetadataProvider metadataProvider)
        {
            var propertiesFields = modelExplorer.GetFieldValue("_properties");
            if (propertiesFields == null && modelExplorer.Model is ICustomTypeDescriptor)
            {
                ICustomTypeDescriptor model = modelExplorer.Model as ICustomTypeDescriptor;
                var metadata = GetMetadataForRuntimeType(modelExplorer, metadataProvider);
                var properties = metadata.Properties;

                var propertyDescriptors = model.GetProperties();

                var _properties = new ModelExplorer[properties.Count];
                for (var i = 0; i < properties.Count; i++)
                {
                    var propertyMetadata = properties[i];
                    PropertyDescriptor propertyDescriptor = null;
                    for (var j = 0; j < propertyDescriptors.Count; j++)
                    {
                        if (string.Equals(
                            propertyMetadata.PropertyName,
                            propertyDescriptors[j].Name,
                            StringComparison.Ordinal))
                        {
                            propertyDescriptor = propertyDescriptors[j];
                            break;
                        }
                    }

                    _properties[i] = CreateExplorerForProperty(metadataProvider, modelExplorer, propertyMetadata, propertyDescriptor);
                }

                modelExplorer.SetFieldValue("_properties", _properties);
            }
            return modelExplorer.Properties;
        }

        private static ModelMetadata GetMetadataForRuntimeType(ModelExplorer modelExplorer, IModelMetadataProvider metadataProvider)
        {
            //The model Explorer always has the parents metadata.
            var metadata = modelExplorer.Metadata;
            if (modelExplorer.ModelType != modelExplorer.Metadata.ModelType)
            {
                 metadata = metadataProvider.GetMetadataForType(modelExplorer.ModelType);
            }

            return metadata;
        }

        private static ModelExplorer CreateExplorerForProperty(
        IModelMetadataProvider metadataProvider,
        ModelExplorer modelExplorer,
        ModelMetadata propertyMetadata,
        PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor == null)
            {
                return new ModelExplorer(metadataProvider, modelExplorer, propertyMetadata, modelAccessor: null);
            }

            var modelAccessor = new Func<object, object>((c) =>
            {
                return c == null ? null : propertyDescriptor.GetValue(((ICustomTypeDescriptor)c).GetPropertyOwner(propertyDescriptor));
            });

            return new ModelExplorer(metadataProvider, modelExplorer, propertyMetadata, modelAccessor);
        }
    }
}
