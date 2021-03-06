﻿using DynamicForms.Attributes.Display.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace DynamicForms.Attributes.Display
{
    public class NoLabelAttribute : Attribute, IDisplayMetadataAttribute
    {
        public bool NoLabel { get; set; } = true;

        public NoLabelAttribute()
        {
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.AdditionalValues["NoLabel"] = NoLabel;
        }
    }
}