using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DynamicForms.Attributes.Display.Common
{
    public interface IDisplayMetadataAttribute
    {
        void TransformMetadata(DisplayMetadataProviderContext context);
    }
}
