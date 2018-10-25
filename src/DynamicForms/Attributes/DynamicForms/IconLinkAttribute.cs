using DynamicForms.Attributes.Display.Common;
using DynamicForms.Attributes.LinkAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace DynamicForms.Attributes.DynamicForms
{
    public class EditLinkAttribute : IconLinkAttribute, IDisplayMetadataAttribute
    {
        public EditLinkAttribute(string actionName, string controllerName)
            : base("Edit", actionName, controllerName, "fa fa-pencil", new { @class = "btn btn-block btn-warning btn-sm mr-2 mb-2" })
        {

        }

        public EditLinkAttribute(string actionName, string controllerName, string ajaxUpdate)
            : base("Edit", actionName, controllerName, "fa fa-pencil", new { @class = "btn btn-block btn-warning btn-sm mr-2 mb-2", data_ajax = "true", data_ajax_method = "GET", data_ajax_mode = "replace", data_ajax_update = ajaxUpdate })
        {

        }
    }

    public class IconLinkAttribute : ActionLinkAttribute, IDisplayMetadataAttribute
    {
        public string IconClass { get; set; }
        public object HtmlAttributes { get; set; }

        public IconLinkAttribute(string linkText, string actionName, string controllerName, string iconClass, object htmlAttributes)
           : base(actionName, controllerName)
        {
            IconClass = iconClass;
            HtmlAttributes = htmlAttributes;
            LinkText = linkText;
        }

        public IconLinkAttribute(string actionName, string controllerName, string iconClass, object htmlAttributes)
            : base(actionName, controllerName)
        {
            IconClass = iconClass;
            HtmlAttributes = htmlAttributes;
        }

        public new void TransformMetadata(DisplayMetadataProviderContext context)
        {
            var propertyAttributes = context.Attributes;
            var modelMetadata = context.DisplayMetadata;
            var propertyName = context.Key.Name;

            modelMetadata.DisplayName = () => "";
            modelMetadata.AdditionalValues["IconClass"] = IconClass;
            modelMetadata.AdditionalValues["HtmlAttributes"] = HtmlAttributes;
            modelMetadata.DataTypeName = "IconLink";

            base.TransformMetadata(context);
        }
    }
}
