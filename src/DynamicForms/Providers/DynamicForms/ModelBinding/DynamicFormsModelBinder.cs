using DynamicForms.ModelBinding;
using DynamicForms.Providers.DynamicForms.Metadata;
using DynamicForms.ViewModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DynamicForms.Providers.DynamicForms.ModelBinding
{
    //[ModelBinder(BinderType = typeof(DynamicFormsModelBinder))] Can add to object OR controller parameter
    public class DynamicFormsModelBinder : IModelBinder
    {
        private readonly IDynamicFormsService _dynamicFormsPresentationService;
        private readonly ModelBinderProviderContext _modelBinderProviderContext;

        public DynamicFormsModelBinder(ModelBinderProviderContext modelBinderProviderContext, IDynamicFormsService dynamicFormsPresentationService)
        {
            _dynamicFormsPresentationService = dynamicFormsPresentationService;
            _modelBinderProviderContext = modelBinderProviderContext;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            //Binder only works for DynamicFormModel
            if (!bindingContext.ModelType.IsAssignableFrom(typeof(DynamicForm)))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            return BindModelCoreAsync(bindingContext);
        }

        private async Task BindModelCoreAsync(ModelBindingContext bindingContext)
        {
            ModelPropertyCollection propertiesRuntime = new ModelPropertyCollection(new List<ModelMetadata>());

            //Type Definition
            if(Int32.TryParse(bindingContext.FieldName, out int index))
            {
                var stackField = typeof(DefaultModelBindingContext).GetField("_stack", BindingFlags.Instance | BindingFlags.NonPublic);
                dynamic stack = stackField.GetValue(bindingContext);

                dynamic collectionModel = null;
                foreach (var stackItem in stack)
                {
                    var prop = stackItem.GetType().GetField("_Model", BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo[] fields = stackItem.GetType().DeclaredFields;
                    var field = fields.Where(f => f.Name == "Model").First();
                    collectionModel = field.GetValue(stackItem);
                }

                if(index < collectionModel.Length)
                {
                    var model = collectionModel[index];
                    bindingContext.Model = model;
                    propertiesRuntime = ((DynamicFormsModelMetadata)bindingContext.ModelMetadata).Contextualize(model);
                }
            }
            else
            {
                propertiesRuntime = ((DynamicFormsModelMetadata)_modelBinderProviderContext.Metadata).Contextualize(bindingContext.Model);
            }

            //New Instance if required
            if (bindingContext.Model == null)
            {
                bindingContext.Model = await CreateModelAsync(bindingContext);
                propertiesRuntime = ((DynamicFormsModelMetadata)bindingContext.ModelMetadata).Contextualize(bindingContext.Model);
            }

            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();

            //https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.Core/src/ModelBinding/Binders/ComplexTypeModelBinderProvider.cs
            for (var i = 0; i < propertiesRuntime.Count; i++)
            {
                var property = propertiesRuntime[i];
                ((DynamicFormsModelMetadata)property).NormalHashing = true;
                propertyBinders.Add(property, _modelBinderProviderContext.CreateBinder(property));
                ((DynamicFormsModelMetadata)property).NormalHashing = false;
            }

            ((DynamicFormsModelMetadata)_modelBinderProviderContext.Metadata).NormalHashing = false;

            var complexModelBinder = new CustomComplexTypeModelBinder(propertyBinders);

            await complexModelBinder.BindModelAsync(bindingContext);
        }

        protected async Task<object> CreateModelAsync(ModelBindingContext bindingContext)
        {
            var formSlugResult = bindingContext.ValueProvider.GetValue(DynamicFormsValueProviderKeys.FormUrlSlug);

            if (formSlugResult == ValueProviderResult.None)
            {
                return new DynamicForm();
            }

            var formSlug = formSlugResult.FirstValue;

            var model = await _dynamicFormsPresentationService.CreateFormModelFromDbAsync(formSlug);

            var modelName = bindingContext.BinderModelName;
            if (string.IsNullOrEmpty(modelName))
            {
                modelName = DynamicFormsValueProviderKeys.FormSubmissionId;
            }

            if (model != null)
            {
                string formSubmissionId = default(string);
                var formSubmissionIdResult = bindingContext.ValueProvider.GetValue(modelName);
                if (formSubmissionIdResult != ValueProviderResult.None)
                {
                    formSubmissionId = formSubmissionIdResult.FirstValue;
                }

                await _dynamicFormsPresentationService.PopulateFormModelFromDbAsync(model, formSubmissionId);
            }

            return model;
        }
    }

    public class CustomComplexTypeModelBinder : ComplexTypeModelBinder
    {
        private IDictionary<ModelMetadata, IModelBinder> _propertyBinders;
        public CustomComplexTypeModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders)
            : base(propertyBinders)
        {
            _propertyBinders = propertyBinders;
        }

        protected override Task BindProperty(ModelBindingContext bindingContext)
        {
            var propertyBinder = _propertyBinders.First(pb => ((DynamicFormsModelMetadata)pb.Key).Identity.Name == ((DynamicFormsModelMetadata)bindingContext.ModelMetadata).Identity.Name).Value;
            return propertyBinder.BindModelAsync(bindingContext);
        }

        protected override void SetProperty(ModelBindingContext bindingContext, string modelName, ModelMetadata propertyMetadata, ModelBindingResult result)
        {
            base.SetProperty(bindingContext, modelName, propertyMetadata, result);
        }
    }
}
