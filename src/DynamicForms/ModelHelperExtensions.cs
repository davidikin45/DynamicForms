using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;

namespace DynamicForms
{
    public static class ModelHelperExtensions
    {

        //Model Type
        public static Type ModelType(this ViewDataDictionary viewData)
        {
            return ModelType(viewData.Model);
        }

        public static Type ModelType(this object model)
        {
            var type = model.GetType();
            var ienum = type.GetInterface(typeof(IEnumerable<>).Name);
            type = ienum != null
              ? ienum.GetGenericArguments()[0]
              : type;
            return type;
        }

        public static String EnumDisplayName(this object e)
        {
            FieldInfo fieldInfo = e.GetType().GetField(e.ToString());
            DisplayAttribute[] displayAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];
            return null != displayAttributes && displayAttributes.Length > 0 ? displayAttributes[0].Name : e.ToString();
        }

        //Values
        public static HtmlString DisplayTextSimple(this IHtmlHelper html, string propertyName)
        {
            return DisplayTextSimple(html, html.ViewData.Model, propertyName);
        }


        public static HtmlString DisplayTextSimple(this IHtmlHelper html, object model, string propertyName)
        {
            var newViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
            var modelExporer = ExpressionMetadataProvider.FromStringExpression(propertyName, newViewData, html.MetadataProvider);

            string value = "";

            if (modelExporer != null)
            {
                value = modelExporer.GetSimpleDisplayText() ?? string.Empty;
                //if (modelExporer.Metadata.HtmlEncode)
                //{
                //    value = HtmlEncoder.Default.Encode(value);
                //}

            }

            return new HtmlString(value);
        }

    }
}
