using DynamicForms.ViewModel.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace DynamicForms.ViewModel
{
    public class DynamicForm : DynamicTypeDescriptorWrapper, ICustomTypeDescriptor
    {
        public DynamicForm()
        {

        }

        public DynamicForm(object model)
            :base(model)
        {
           
        }

    }


}
