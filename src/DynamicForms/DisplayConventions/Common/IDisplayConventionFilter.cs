using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicForms.Attributes.Common
{
    public interface IDisplayConventionFilter
    {
        void TransformMetadata(DisplayMetadataProviderContext context);
    }
}
