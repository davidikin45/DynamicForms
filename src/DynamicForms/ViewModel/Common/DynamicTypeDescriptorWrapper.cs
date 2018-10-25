using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicForms.ViewModel.Common
{
    //https://weblogs.asp.net/bleroy/fun-with-c-4-0-s-dynamic
    public class DynamicTypeDescriptorWrapper : DynamicObject, ICustomTypeDescriptor
    {
        protected IDynamicMetaObjectProvider _dynamic;

        public DynamicTypeDescriptorWrapper(IDynamicMetaObjectProvider dynamicObject)
        {
            _dynamic = dynamicObject;
        }

        public DynamicTypeDescriptorWrapper()
        {
            _dynamic = new ExpandoObject();
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return new AttributeCollection();
        }

        public string GetClassName()
        {
            return "dynamic";
        }

        public string GetComponentName()
        {
            return "Dynamic";
        }

        public TypeConverter GetConverter()
        {
            return null;
        }

        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(new EventDescriptor[] { });
        }

        public EventDescriptorCollection GetEvents()
        {
            return new EventDescriptorCollection(new EventDescriptor[] { });
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        private Dictionary<string, DynamicPropertyDescriptor> properties = new Dictionary<string, DynamicPropertyDescriptor>();
        public PropertyDescriptorCollection GetProperties()
        {
            var meta = _dynamic.GetMetaObject(Expression.Constant(_dynamic));
            var memberNames = meta.GetDynamicMemberNames();

            var props = new PropertyDescriptorCollection(new PropertyDescriptor[] { });

            foreach (var memberName in memberNames)
            {
                if (!properties.ContainsKey(memberName))
                {
                    var newProperty = new DynamicPropertyDescriptor(_dynamic, memberName);
                    properties.Add(memberName, newProperty);
                }

                props.Add(properties[memberName]);
            }

            return props;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dynamic;
        }
        #endregion

        #region Helper Methods
        public bool IsCollectionProperty(string propertyName)
        {
            var property = GetProperties().Find(propertyName, true);
            return IsCollection(property.PropertyType);
        }

        public bool IsCollection(Type type)
        {
            return type.GetInterfaces().Where(x => x.GetTypeInfo().IsGenericType).Any(x => x.GetGenericTypeDefinition() == typeof(ICollection<>) && !x.GetGenericArguments().Contains(typeof(Byte)));
        }
        #endregion

        #region Property Index Accessor
        public object this[string key]
        {
            get
            {
                //Must exist for get
                return GetMember(key);
            }
            set
            {
                if (ContainsProperty(key))
                {
                    SetMember(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }
        #endregion

        #region Add Attribute
        public void AddAttribute<TAttribute>(string propertyName, TAttribute attribute) where TAttribute : Attribute
        {
            var property = (DynamicPropertyDescriptor)GetProperties()[propertyName];
            properties.Remove(propertyName);
            var newPropertyWithAttribute = new DynamicPropertyDescriptor(property, new Attribute[] { attribute });
            properties.Add(propertyName, newPropertyWithAttribute);
        }

        public IEnumerable<TAttribute> GetAttribute<TAttribute>(string propertyName) where TAttribute : Attribute
        {
            var property = (DynamicPropertyDescriptor)GetProperties()[propertyName];
            return property.Attributes.OfType<TAttribute>();
        }
        #endregion

            #region Contains Property
        public bool ContainsProperty(string property)
        {
            return GetProperties().Find(property, true) != null;
        }
        #endregion

        #region Add/Remove Properties
        public void Add(string property, object value)
        {
            if (!(_dynamic is IDictionary<string, object>))
            {
                throw new Exception("Can only add properties to Expando Objects");
            }

            var dict = _dynamic as IDictionary<string, object>;
            AddProperty(dict, property, value);
        }

        private void AddProperty(IDictionary<string, object> dict, string property, object value)
        {
            dict.Add(property, value);
        }

        public void Remove(string property)
        {
            if (!(_dynamic is IDictionary<string, object>))
            {
                throw new Exception("Can only remove properties from Expando Objects");
            }

            var dict = _dynamic as IDictionary<string, object>;
            RemoveProperty(dict, property);
        }

        private void RemoveProperty(IDictionary<string, object> dict, string property)
        {
            dict.Remove(property);
        }
        #endregion

        #region Dynamic Methods
        private object GetMember(string propertyName)
        {
            return GetProperties().Find(propertyName, true).GetValue(_dynamic);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (ContainsProperty(binder.Name))
            {
                result = GetMember(binder.Name);
                return true;
            }

            // failed to retrieve a property
            result = null;
            return false;
        }

        private void SetMember(string propertyName, object value)
        {
            var property = GetProperties().Find(propertyName, true);
            property.SetValue(_dynamic, value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // first check to see if there's a native property to set
            if(ContainsProperty(binder.Name))
            {
                SetMember(binder.Name, value);
                return true;
            }
            else
            {
                Add(binder.Name, value);
                return true;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var meta = _dynamic.GetMetaObject(Expression.Constant(_dynamic));
            var memberNames = meta.GetDynamicMemberNames();
            return memberNames;
        }
        #endregion
    }
}