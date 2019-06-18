using DynamicForms.Attributes.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DynamicForms.DisplayConventions
{
    public class DisableConvertEmptyStringToNull : IDisplayConventionFilter
    {
        public DisableConvertEmptyStringToNull()
        {

        }

        public void TransformMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.MetadataKind == ModelMetadataKind.Property)
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
