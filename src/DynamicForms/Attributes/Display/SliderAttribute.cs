﻿using DynamicForms.Attributes.Display.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;

namespace DynamicForms.Attributes.Display
{
    public class SliderAttribute : Attribute, IDisplayMetadataAttribute
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 100;

        public SliderAttribute()
        {

        }

        public SliderAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DataTypeName = "Slider";
            modelMetadata.AdditionalValues["Min"] = Min;
            modelMetadata.AdditionalValues["Max"] = Max;
        }
    }
}
