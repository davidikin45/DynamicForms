﻿using DynamicForms.ModelBinding.HybridBinding.Attributes;
using DynamicForms.ModelBinding.HybridBinding.BindingSources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace DynamicForms.ModelBinding.HybridBinding.Conventions
{
    public class FromBodyAndOtherSourcesAttributeConvention : IParameterModelConvention, IActionModelConvention, IControllerModelConvention, IApplicationModelConvention
    {
        private readonly bool _enableForParametersWithNoBinding;
        private readonly bool _enableForParametersWithFormRouteQueryBinding;
        private readonly bool _changeFromBodyBindingsToFromBodyFormAndRouteQueryBinding;

        public FromBodyAndOtherSourcesAttributeConvention(bool enableForParametersWithNoBinding, bool enableForParametersWithFormRouteQueryBinding,  bool changeFromBodyBindingsToFromBodyFormAndRouteQueryBinding)
        {
            _enableForParametersWithNoBinding = enableForParametersWithNoBinding;
            _enableForParametersWithFormRouteQueryBinding = enableForParametersWithFormRouteQueryBinding;
            _changeFromBodyBindingsToFromBodyFormAndRouteQueryBinding = changeFromBodyBindingsToFromBodyFormAndRouteQueryBinding;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controler in application.Controllers)
            {
                foreach (var action in controler.Actions)
                {
                    foreach (var paramater in action.Parameters)
                    {
                        Apply(paramater);
                    }
                }
            }
        }

        public void Apply(ActionModel action)
        {
            var antiForgeryTokenFilters = action.Filters.Where(f => f is ValidateAntiForgeryTokenAttribute || f is AutoValidateAntiforgeryTokenAttribute).ToList();
            if(antiForgeryTokenFilters.Any())
            {
                antiForgeryTokenFilters.ForEach(af => action.Filters.Remove(af));
                action.Filters.Add(new AutoValidateFormAntiforgeryTokenAttribute());
            }

            Apply(action.Controller);
        }

        public void Apply(ControllerModel controller)
        {

            var antiForgeryTokenFilters = controller.Filters.Where(f => f is ValidateAntiForgeryTokenAttribute || f is AutoValidateAntiforgeryTokenAttribute).ToList();
            if (antiForgeryTokenFilters.Any())
            {
                antiForgeryTokenFilters.ForEach(af => controller.Filters.Remove(af));
                controller.Filters.Add(new AutoValidateFormAntiforgeryTokenAttribute());
            }
        }

        public void Apply(ParameterModel parameter)
        {
            if (parameter.BindingInfo == null)
            {
                if(_enableForParametersWithNoBinding)
                {
                    parameter.BindingInfo = new BindingInfo();
                    parameter.BindingInfo.BinderType = typeof(BodyAndOtherSourcesModelBinder);

                    Apply(parameter.Action);
                }
            }
            else if (parameter.BindingInfo.BinderType == null && (parameter.BindingInfo.BindingSource == null || parameter.BindingInfo.BindingSource == BindingSource.Form || parameter.BindingInfo.BindingSource == BindingSource.Path || parameter.BindingInfo.BindingSource == BindingSource.Query || parameter.BindingInfo.BindingSource == BindingSource.ModelBinding))
            {
                if (_enableForParametersWithFormRouteQueryBinding)
                {
                    if (parameter.BindingInfo.BindingSource == BindingSource.Form)
                        parameter.BindingInfo.BindingSource = BodyOrBindingSource.BodyOrForm;
                    else if (parameter.BindingInfo.BindingSource == BindingSource.Path)
                        parameter.BindingInfo.BindingSource = BodyAndBindingSource.BodyAndPath;
                    else if (parameter.BindingInfo.BindingSource == BindingSource.Query)
                        parameter.BindingInfo.BindingSource = BodyAndBindingSource.BodyAndQuery;
                    else if (parameter.BindingInfo.BindingSource == BindingSource.ModelBinding)
                        parameter.BindingInfo.BindingSource = BodyAndBindingSource.BodyAndModelBinding;

                    parameter.BindingInfo.BinderType = typeof(BodyAndOtherSourcesModelBinder);

                    Apply(parameter.Action);
                }
           }
            else if(parameter.BindingInfo.BinderType == null && parameter.BindingInfo.BindingSource == BindingSource.Body)
            {
                if(_changeFromBodyBindingsToFromBodyFormAndRouteQueryBinding)
                {
                    parameter.BindingInfo.BindingSource = BodyAndBindingSource.BodyAndModelBinding;
                    parameter.BindingInfo.BinderType = typeof(BodyAndOtherSourcesModelBinder);

                    Apply(parameter.Action);
                }
            }
        }
    }
}
