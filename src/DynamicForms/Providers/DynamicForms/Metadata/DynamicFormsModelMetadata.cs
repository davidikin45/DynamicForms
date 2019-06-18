using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DynamicForms.Providers.DynamicForms.Metadata
{
    //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/ModelBinding/Metadata/DefaultModelMetadata.cs
    public class DynamicFormsModelMetadata : DefaultModelMetadata
    {
        public new ModelMetadataIdentity Identity { get { return base.Identity; } }

        private readonly IDynamicFormsModelMetadataProviderSingleton _customProvider;
        public DynamicFormsModelMetadata(IDynamicFormsModelMetadataProviderSingleton provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details)
            : base(provider, detailsProvider, details)
        {
            _customProvider = provider;
        }

        public DynamicFormsModelMetadata(IDynamicFormsModelMetadataProviderSingleton provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider)
             : base(provider, detailsProvider, details, modelBindingMessageProvider)
        {
            _customProvider = provider;
        }

        //Generally the Properties are based on the Model Type and not the Model instance.
        public ModelPropertyCollection Contextualize(object model)
        {
            if (!(model is ICustomTypeDescriptor))
            {
                return Properties;
            }
            else
            {
                var propertiesField = typeof(DynamicFormsModelMetadata).BaseType.GetField("_properties", BindingFlags.Instance | BindingFlags.NonPublic);

                IEnumerable<ModelMetadata> properties = _customProvider.GetMetadataForProperties(ModelType, model as ICustomTypeDescriptor);
                properties = properties.OrderBy(p => p.Order);
                propertiesField.SetValue(this, new ModelPropertyCollection(properties));

                return (ModelPropertyCollection)propertiesField.GetValue(this);
            }
        }

        public override ModelPropertyCollection Properties
        {
            get
            {
               
                return base.Properties;
            }
        }

        public override ModelMetadata ElementMetadata
        {
            get
            {
                if (ElementType != null)
                {
                    if (ElementType.GetInterfaces().Contains(typeof(ICustomTypeDescriptor)))
                    {
                        return _customProvider.GetMetadataForType(ElementType);
                    }
                    else
                    {
                        return base.ElementMetadata;
                    }
                }

                return null;
            }
        }


        public bool NormalHashing { get; set; }
        //1.
        public override int GetHashCode()
        {
            if (NormalHashing || MetadataKind != ModelMetadataKind.Property || !ContainerType.GetInterfaces().Contains(typeof(ICustomTypeDescriptor)))
            {
                return base.GetHashCode();
            }
            else
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + base.GetHashCode();
                    hash = hash * 31 + new Random().Next();
                    return hash;
                };
            }
        }

        //2.
        public override bool Equals(object obj)
        {
            if (NormalHashing || MetadataKind != ModelMetadataKind.Property || !ContainerType.GetInterfaces().Contains(typeof(ICustomTypeDescriptor)))
            {
                return base.Equals(obj);
            }
            else
            {
               return false;
            }
        }
    }
}
