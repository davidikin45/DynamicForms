using DynamicForms.Attributes.Display;
using DynamicForms.Attributes.DynamicForms;
using DynamicForms.Attributes.Validation;
using DynamicForms.ViewModel;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DynamicForms.Web.Controllers
{
    [Route("forms")]
    public class DynamicFormsController : Controller
    {
        #region Edit
        [Route("dynamic")]
        public IActionResult Edit()
        {
            var form = SetupForm();
            return View("_DynamicFormEdit", form);
        }
        #endregion

        #region Update
        [ValidateAntiForgeryToken]
        [Route("dynamic")]
        [HttpPost]
        private IActionResult Update()
        {
            var form = SetupForm();
            this.BindForm(form);

            if (TryValidateModel(form))
            {

            }

            return View("_DynamicFormEdit", form);
        }
        #endregion

        private DynamicForm SetupForm()
        {

            //1. Setup Form definition
            dynamic model = new DynamicForm();
            model.Add("Text", "");
            model.Add("Email", "");
            model.Add("Website", "");
            model.Add("PhoneNumber", "");
            model.Add("TextArea", "");
            model.Add("Number", 0);
            model.Add("Slider", 50);

            decimal currency = 0;
            model.Add("Currency", currency);
            model.Add("Date", new DateTime());
            model.Add("DateTime", new DateTime());
            model.Add("Dropdown", "");
            model.Add("DropdownMany", new List<string>());
            model.Add("RadioList", "");
            model.Add("RadioListButtons", "");
            model.Add("CheckboxList", new List<string>());
            model.Add("CheckboxListButtons", new List<string>());
            model.Add("Checkbox", false);
            model.Add("YesButton", false);

            model.Add("YesNo", "");
            model.Add("YesNoButtons", "");

            model.Add("YesNoButtonsBoolean", false);

            model.Add("TrueFalse", "");
            model.Add("TrueFalseButtons", "");
            model.Add("TrueFalseButtonsBoolean", false);

            FormFile formFile = new FormFile(null, 0, 0, "", "");
            model.Add("File", formFile);
            model.Add("MultipleFiles", new List<FormFile>() { });
            model.Add("MultipleMediaFiles", new List<FormFile>() { });

            //2. Add Display and Validation
            model.AddAttribute("Text", new DisplayAttribute() { Name = "What is your Name?" });
            model.AddAttribute("Email", new DataTypeAttribute(DataType.EmailAddress));
            model.AddAttribute("Email", new HelpTextAttribute("Your personal email please"));
            model.AddAttribute("PhoneNumber", new DataTypeAttribute(DataType.PhoneNumber));
            model.AddAttribute("Website", new DataTypeAttribute(DataType.Url));
            model.AddAttribute("TextArea", new MultilineTextAttribute(5));

            model.AddAttribute("Number", new NumberValidatorAttribute());

            model.AddAttribute("Slider", new SliderAttribute(0, 100));

            //text-success
            model.AddAttribute("SectionHeading", new HeadingAttributeH3("text-danger"));

          //  model.AddAttribute("Dropdown", new DropdownAttribute(Type.GetType("DND.Domain.Blog.Tags.Tag, DND.Domain.Blog"), "Name", "Name"));
           // model.AddAttribute("DropdownMany", new DropdownAttribute(Type.GetType("DND.Domain.Blog.Tags.Tag, DND.Domain.Blog"), "Name", "Name"));

           // model.AddAttribute("RadioList", new CheckboxOrRadioAttribute(Type.GetType("DND.Domain.Blog.Tags.Tag, DND.Domain.Blog"), "Name", "Name"));
            model.AddAttribute("RadioListButtons", new CheckboxOrRadioButtonsAttribute(new List<string>() { "Option 1", "Option 2", "Option 3", "Option 4" }));

          //  model.AddAttribute("CheckboxList", new CheckboxOrRadioAttribute(Type.GetType("DND.Domain.Blog.Tags.Tag, DND.Domain.Blog"), "Name", "Name"));
            //wrapper.AddAttribute("CheckboxList", new CheckboxOrRadioInlineAttribute());
            model.AddAttribute("CheckboxList", new LimitCountAttribute(3, 5));

            model.AddAttribute("CheckboxListButtons", new CheckboxOrRadioButtonsAttribute(new List<string>() { "Option 1", "Option 2", "Option 3", "Option 4" }));

            model.AddAttribute("Currency", new DataTypeAttribute(DataType.Currency));

            model.AddAttribute("Date", new DataTypeAttribute(DataType.Date));
            model.AddAttribute("Date", new AgeValidatorAttribute(18));

            model.AddAttribute("DateTime", new DataTypeAttribute(DataType.DateTime));

            model.AddAttribute("YesButton", new BooleanYesButtonAttribute());

            model.AddAttribute("YesNo", new YesNoCheckboxOrRadioAttribute());
            model.AddAttribute("YesNo", new CheckboxOrRadioInlineAttribute());

            model.AddAttribute("YesNoButtons", new YesNoCheckboxOrRadioButtonsAttribute());

            model.AddAttribute("YesNoButtonsBoolean", new BooleanYesNoButtonsAttribute());

            model.AddAttribute("TrueFalse", new TrueFalseCheckboxOrRadioAttribute());
            model.AddAttribute("TrueFalse", new CheckboxOrRadioInlineAttribute());

            model.AddAttribute("TrueFalseButtons", new TrueFalseCheckboxOrRadioButtonsAttribute());

            model.AddAttribute("TrueFalseButtonsBoolean", new BooleanTrueFalseButtonsAttribute());

            model.AddAttribute("MultipleMediaFiles", new FileImageAudioVideoAcceptAttribute());

            model.AddAttribute("Submit", new NoLabelAttribute());
            model.AddAttribute("Submit", new SubmitButtonAttribute("btn btn-block btn-success"));

            return model;
        }
    }
}
