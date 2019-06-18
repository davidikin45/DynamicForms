using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DynamicForms.Providers.DynamicForms.Metadata
{

    //ModelMetadata = Type
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/ModelBinding/Metadata/DefaultModelMetadata.cs

    //ModelExplorer = Type + Model Instance
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ViewDataDictionary.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ModelExplorer.cs

    //Render Object
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateRenderer.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ViewResultExecutor.cs

    //Default Templates
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/DefaultEditorTemplates.cs
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/DefaultDisplayTemplates.cs

    //View Engine
    //https://github.com/aspnet/AspNetCore/blob/19c9010c2fc44f6fa3952c3f46d1b6e86e45fa8c/src/Mvc/Mvc.Razor/src/RazorViewEngine.cs

    //Html Helper
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/HtmlHelper.cs

    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/TemplateBuilder.cs
    //New model Explorer generated.
    //  viewData.ModelExplorer = _modelExplorer.GetExplorerForModel(_model); 

    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.ViewFeatures/src/ModelExplorer.cs
    //
    public class DynamicFormsModelMetadataProviderSingleton : DefaultModelMetadataProvider, IDynamicFormsModelMetadataProviderSingleton
    {
        public DynamicFormsModelMetadataProviderSingleton(ICompositeMetadataDetailsProvider detailsProvider)
            : base(detailsProvider)
        {

        }

        public DynamicFormsModelMetadataProviderSingleton(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor)
            : base(detailsProvider, optionsAccessor)
        {

        }

        //https://stackoverflow.com/questions/47296817/getting-imetadatadetailsproviders-to-run-more-than-once-in-asp-net-core

        //ModelMetadataProviderExtensions.GetModelExplorerForType calls back into this message
        public override ModelMetadata GetMetadataForType(Type modelType)
        {
            //  Optimization for intensively used System.Object
            if(!modelType.GetInterfaces().Contains(typeof(ICustomTypeDescriptor)))
            {
                return base.GetMetadataForType(modelType);
            }

            var identity = ModelMetadataIdentity.ForType(modelType);
            DefaultMetadataDetails details = CreateTypeDetails(identity);

            ////Generally it Lazy Loads these.
            //var displayContext = new DisplayMetadataProviderContext(identity, details.ModelAttributes);
            ////  Here your implementation of IDisplayMetadataProvider will be called
            //DetailsProvider.CreateDisplayMetadata(displayContext);
            //details.DisplayMetadata = displayContext.DisplayMetadata;

            ////Generally it Lazy Loads these.
            //var validationContext = new ValidationMetadataProviderContext(identity, details.ModelAttributes);
            ////  Here your implementation of IValidationMetadataProvider will be called
            //DetailsProvider.CreateValidationMetadata(validationContext);
            //details.ValidationMetadata = validationContext.ValidationMetadata;

            var modelMetadata = CreateModelMetadata(details);


            return modelMetadata;
        }

        protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        {
            return new DynamicFormsModelMetadata(this, DetailsProvider, entry, ModelBindingMessageProvider);
        }

        public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType, ICustomTypeDescriptor model)
        {
            var key = ModelMetadataIdentity.ForType(modelType);
            var propertyDetails = CreatePropertyDetails(key, model as ICustomTypeDescriptor);

            var properties = new ModelMetadata[propertyDetails.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                propertyDetails[i].ContainerMetadata = GetMetadataForType(modelType);
                properties[i] = CreateModelMetadata(propertyDetails[i]);
            }

            return properties;
        }

        protected DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key, ICustomTypeDescriptor model)
        {
            var properties = model.GetProperties();

            var propertyEntries = new List<DefaultMetadataDetails>(properties.Count);
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyDescriptor = properties[i];

                var propertyKey = ModelMetadataIdentity.ForProperty(
                    propertyDescriptor.PropertyType,
                    propertyDescriptor.Name,
                    key.ModelType);

                var propertyEntry = CreateSinglePropertyDetails(propertyKey, propertyDescriptor);
                propertyEntries.Add(propertyEntry);
            }

            return propertyEntries.ToArray();
        }

        private DefaultMetadataDetails CreateSinglePropertyDetails(
           ModelMetadataIdentity propertyKey,
           PropertyDescriptor propertyDescriptor)
        {
            var containerType = propertyKey.ContainerType;

            var attributes = GetAttributesForProperty(
                containerType,
               propertyDescriptor,
                propertyKey.ModelType);

            var propertyEntry = new DefaultMetadataDetails(propertyKey, attributes);

            var modelAccessor = new Func<object, object>((c) =>
            {
                return c == null ? null : propertyDescriptor.GetValue(((ICustomTypeDescriptor)c).GetPropertyOwner(propertyDescriptor));
            });

            propertyEntry.PropertyGetter = modelAccessor;

            if (!propertyDescriptor.IsReadOnly)
            {
                var modelSetter = new Action<object, object>((c, v) =>
                {
                    propertyDescriptor.SetValue(((ICustomTypeDescriptor)c).GetPropertyOwner(propertyDescriptor), v);
                });

                propertyEntry.PropertySetter = modelSetter;
            }

            return propertyEntry;
        }

        public ModelAttributes GetAttributesForProperty(Type containerType, PropertyDescriptor property, Type modelType)
        {
            if (containerType == null)
            {
                throw new ArgumentNullException(nameof(containerType));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyAttributes = new List<Attribute>();
            for (int i = 0; i < property.Attributes.Count; i++)
            {
                propertyAttributes.Add(property.Attributes[i]);
            }

            var ienumAttributes = propertyAttributes.Cast<Attribute>();

            var typeAttributes = modelType.GetCustomAttributes();

            var metadataType = GetMetadataType(containerType);
            if (metadataType != null)
            {
                var metadataProperty = metadataType.GetRuntimeProperty(property.Name);
                if (metadataProperty != null)
                {
                    ienumAttributes = ienumAttributes.Concat(metadataProperty.GetCustomAttributes());
                }
            }

            return new ModelAttributes(ienumAttributes, typeAttributes);
        }

        private Type GetMetadataType(Type type)
        {
            return type.GetCustomAttribute<ModelMetadataTypeAttribute>()?.MetadataType;
        }
    }
}
