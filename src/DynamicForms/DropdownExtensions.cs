using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicForms
{
    public static class DropdownExtensions
    {
        public static IList<SelectListItem> GetSelectList(this IHtmlHelper<dynamic> htmlHelper, string propertyName, bool selectedOnly = false)
        {
            var modelExplorer = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider);
            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = modelExplorer.Metadata;

            var dropdownModelType = ((Type)metadata.AdditionalValues["DropdownModelType"]);
            var keyProperty = ((string)metadata.AdditionalValues["KeyProperty"]);
            var valueProperty = ((string)metadata.AdditionalValues["DisplayExpression"]);
            var bindingProperty = ((string)metadata.AdditionalValues["BindingProperty"]);

            var orderByProperty = ((string)metadata.AdditionalValues["OrderByProperty"]);
            var orderByType = ((string)metadata.AdditionalValues["OrderByType"]);

            var physicalFilePath = ((string)metadata.AdditionalValues["PhysicalFilePath"]);

            var physicalFolderPath = ((string)metadata.AdditionalValues["PhysicalFolderPath"]);

            var nullable = ((bool)metadata.AdditionalValues["Nullable"]);

            var options = ((IEnumerable<string>)metadata.AdditionalValues["Options"]);

            Type propertyType = GetNonNullableModelType(metadata);
            List<SelectListItem> items = new List<SelectListItem>();
            List<string> ids = new List<string>();

            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                if (modelExplorer.Model != null)
                {
                    foreach (var val in (IEnumerable)modelExplorer.Model)
                    {
                        if (val != null)
                        {
                            if (!string.IsNullOrWhiteSpace(bindingProperty))
                            {
                                ids.Add(val.GetPropValue(bindingProperty).ToString());
                            }
                            else
                            {
                                ids.Add(val.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                if (modelExplorer.Model != null)
                {
                    if (!string.IsNullOrWhiteSpace(bindingProperty))
                    {
                        ids.Add(modelExplorer.Model.GetPropValue(bindingProperty).ToString());
                    }
                    else
                    {
                        ids.Add(modelExplorer.Model.ToString());
                    }
                }
            }

            if (metadata.DataTypeName == "ModelRepeater")
            {
                foreach (var item in htmlHelper.ViewData.Model)
                {
                    var itemObject = (Object)item;

                    items.Add(new SelectListItem()
                    {
                        Text = GetDisplayString(htmlHelper, item, valueProperty),
                        Value = itemObject.GetPropValue(keyProperty) != null ? itemObject.GetPropValue(keyProperty).ToString() : "",
                        Selected = itemObject.GetPropValue(keyProperty) != null && ids.Contains(itemObject.GetPropValue(keyProperty).ToString())
                    });
                }
            }
            else
            {
                //db
                if (options == null)
                {

                   
                }
                else
                {
                    options.ToList().ForEach(item =>
                      items.Add(new SelectListItem()
                      {
                          Text = item,
                          Value = item,
                          Selected = ids.Contains(item)
                      }));
                }
            }

            if (metadata.IsNullableValueType || nullable)
            {
                items.Insert(0, new SelectListItem { Text = "", Value = "" });
            }

            return items;
        }

        private static string GetDisplayString(IHtmlHelper htmlHelper, dynamic obj, string displayExpression)
        {
            string value = displayExpression;

            if (!value.Contains("{") && !value.Contains(" "))
            {
                value = "{" + value + "}";
            }

            var replacementTokens = GetReplacementTokens(value);
            foreach (var token in replacementTokens)
            {
                var propertyName = token.Substring(1, token.Length - 2);
                var displayString = "";
                //var displayString = HtmlHelperExtensions.ToString(ModelHelperExtensions.Display(htmlHelper, obj, propertyName));
                value = value.Replace(token, displayString);
            }

            return value;
        }

        private static List<String> GetReplacementTokens(String str)
        {
            Regex regex = new Regex(@"{(.*?)}", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(str);

            // Results include braces (undesirable)
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }

        public static IHtmlContent DropDownList(this IHtmlHelper<dynamic> htmlHelper, string propertyName, object htmlAttributes = null)
        {
            IList<SelectListItem> items = GetSelectList(htmlHelper, propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ListBox(propertyName, items, htmlAttributes);
            }
            else
            {
                return htmlHelper.DropDownList(propertyName, items, htmlAttributes);
            }
        }

        public static IHtmlContent Checkbox(this IHtmlHelper<dynamic> htmlHelper, string propertyName, object htmlAttributes = null)
        {
            IList<SelectListItem> items = GetSelectList(htmlHelper, propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            bool inline = false;
            if (metadata.AdditionalValues.ContainsKey("ModelCheckboxOrRadioInline"))
            {
                inline = ((bool)metadata.AdditionalValues["ModelCheckboxOrRadioInline"]);
            }

            var sb = new StringBuilder();
            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ValueCheckboxList(propertyName, items, inline);
            }
            else
            {
                return htmlHelper.ValueRadioList(propertyName, items, inline);
            }
        }

        public static IHtmlContent CheckboxButtons(this IHtmlHelper<dynamic> htmlHelper, string propertyName, bool groupRadioButtons, object htmlAttributes = null, object labelCheckboxHtmlAttributes = null, object labelRadioHtmlAttributes = null)
        {
            IList<SelectListItem> items = GetSelectList(htmlHelper, propertyName);

            Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata metadata = ExpressionMetadataProvider.FromStringExpression(propertyName, htmlHelper.ViewData, htmlHelper.MetadataProvider).Metadata;
            Type propertyType = GetNonNullableModelType(metadata);

            var sb = new StringBuilder();
            if (propertyType != typeof(string) && (propertyType.GetInterfaces().Contains(typeof(IEnumerable))))
            {
                return htmlHelper.ValueCheckboxButtonList(propertyName, items, htmlAttributes, labelCheckboxHtmlAttributes);
            }
            else
            {
                return htmlHelper.ValueRadioButtonList(propertyName, items, groupRadioButtons, htmlAttributes, labelRadioHtmlAttributes);
            }
        }

        private static Type GetNonNullableModelType(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;
            Type underlyingType = Nullable.GetUnderlyingType(realModelType);


            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }

            return realModelType;
        }
    }
}
