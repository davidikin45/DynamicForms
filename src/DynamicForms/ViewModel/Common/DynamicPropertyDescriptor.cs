using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;

namespace DynamicForms.ViewModel.Common
{
    public class DynamicPropertyDescriptor : PropertyDescriptor {
        private string _propertyName;
        private IDynamicMetaObjectProvider _owner;

        public DynamicPropertyDescriptor(DynamicPropertyDescriptor oldDynamicPropertyDescriptor, Attribute[] newAttrs) : base(oldDynamicPropertyDescriptor, newAttrs)
        {
            _propertyName = oldDynamicPropertyDescriptor.Name;
            _owner = oldDynamicPropertyDescriptor._owner;
        }

        public DynamicPropertyDescriptor(IDynamicMetaObjectProvider owner, string propertyName) : base(propertyName, null) {
            _propertyName = propertyName;
            _owner = owner;
        }

        public override bool Equals(object obj) {
            DynamicPropertyDescriptor other = obj as DynamicPropertyDescriptor;
            return other != null && other._propertyName.Equals(_propertyName);
        }

        public override int GetHashCode() { return _propertyName.GetHashCode(); }

        public override bool IsReadOnly { get { return false; } }

        public override void ResetValue(object component) { }

        public override bool CanResetValue(object component) { return false; }

        public override bool ShouldSerializeValue(object component) {
            return true;
        }

        public override Type ComponentType {
            get { return _owner.GetType(); }
        }

        public override Type PropertyType { get { return GetValue(_owner).GetType(); } }

        public override object GetValue(object component) {
            if (_owner != component) throw new InvalidOperationException("GetValue can only be used with the descriptor's owner.");
            var value = DynamicHelper.GetValue(component, _propertyName);
            return value;
        }

        public override void SetValue(object component, object value) {
            if (_owner != component) throw new InvalidOperationException("SetValue can only be used with the descriptor's owner.");
            OnValueChanged(component, EventArgs.Empty);
            DynamicHelper.SetValue(component, _propertyName, value);
        }
    }
}
