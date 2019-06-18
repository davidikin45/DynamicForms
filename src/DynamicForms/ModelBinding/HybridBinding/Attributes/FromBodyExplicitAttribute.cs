using DynamicForms.ModelBinding.HybridBinding.BindingSources;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace DynamicForms.ModelBinding.HybridBinding.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromBodyExplicitAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => BodyExplicitBindingSource.Body;
    }
}
