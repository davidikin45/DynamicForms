using DynamicForms.Attributes.Common;
using DynamicForms.Attributes.Display.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DynamicForms.Providers.Attributes
{
    public class AttributeMetadataProvider : IDisplayMetadataProvider
    {
        public AttributeMetadataProvider() { }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.PropertyAttributes != null)
            {
                foreach (object propAttr in context.PropertyAttributes)
                {
                    if(propAttr is IDisplayMetadataAttribute)
                    {
                        ((IDisplayMetadataAttribute)propAttr).TransformMetadata(context);
                    }
                }
            }
        }
    }
}
