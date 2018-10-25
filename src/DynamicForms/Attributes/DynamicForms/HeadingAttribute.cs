using DynamicForms.Attributes.Display;
using DynamicForms.Attributes.Display.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DynamicForms.Attributes.DynamicForms
{
    public class HeadingAttributeH1 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH1(string @class)
            :base("h1", @class)
        {

        }

        public HeadingAttributeH1(string headingText, string @class)
             : base(headingText, "h1", @class)
        {

        }
    }

    public class HeadingAttributeH2 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH2(string @class)
            : base("h2", @class)
        {

        }

        public HeadingAttributeH2(string headingText, string @class)
             : base(headingText, "h2", @class)
        {

        }
    }

    public class HeadingAttributeH3 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH3(string @class)
            : base("h3", @class)
        {

        }

        public HeadingAttributeH3(string headingText, string @class)
             : base(headingText, "h3", @class)
        {

        }
    }

    public class HeadingAttributeH4 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH4(string @class)
            : base("h4", @class)
        {

        }

        public HeadingAttributeH4(string headingText, string @class)
             : base(headingText, "h4", @class)
        {

        }
    }

    public class HeadingAttributeH5 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH5(string @class)
            : base("h5", @class)
        {

        }

        public HeadingAttributeH5(string headingText, string @class)
             : base(headingText, "h5", @class)
        {

        }
    }

    public class HeadingAttributeH6 : HeadingAttribute, IDisplayMetadataAttribute
    {
        public HeadingAttributeH6(string @class)
            : base("h6", @class)
        {

        }

        public HeadingAttributeH6(string headingText, string @class)
             : base(headingText, "h6", @class)
        {

        }
    }

    public class HeadingAttribute : NoLabelAttribute, IDisplayMetadataAttribute
    {
        public string HeadingText { get; set; }
        public string TagName { get; set; }
        public string Class { get; set; }

        public HeadingAttribute(string tagName, string @class)
        {
            TagName = tagName;
            Class = @class;
        }

        public HeadingAttribute(string headingText, string tagName, string @class)
        {
            HeadingText = headingText;
            TagName = tagName;
            Class = @class;
        }

        public new void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DataTypeName = "Heading";

            modelMetadata.AdditionalValues["HeadingText"] = HeadingText;
            modelMetadata.AdditionalValues["TagName"] = TagName;
            modelMetadata.AdditionalValues["Class"] = Class;

            base.TransformMetadata(context);
        }
    }
}