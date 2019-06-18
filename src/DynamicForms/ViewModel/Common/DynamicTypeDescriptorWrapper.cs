using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicForms.ViewModel.Common
{
    //Expando
    //It's a sealed type so you can't use it as a base class
    //It doesn't serialize to XML

    //https://weblogs.asp.net/bleroy/fun-with-c-4-0-s-dynamic
    public class DynamicTypeDescriptorWrapper : DynamicObject, ICustomTypeDescriptor
    {
        object Instance;
        Type InstanceType;

        Dictionary<string, PropertyDescriptor> InstancePropertyDescriptionCollection
        {
            get
            {
                if (_InstancePropertyDescriptionCollection == null && Instance != null)
                {
                    _InstancePropertyDescriptionCollection = new Dictionary<string, PropertyDescriptor>();

                    var properties = TypeDescriptor.GetProperties(InstanceType);
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var prop = properties[i];
                        _InstancePropertyDescriptionCollection.Add(prop.Name, prop);
                    }
                }

                return _InstancePropertyDescriptionCollection;
            }
        }
        Dictionary<string, PropertyDescriptor> _InstancePropertyDescriptionCollection;

        PropertyInfo[] InstancePropertyInfo
        {
            get
            {
                if (_InstancePropertyInfo == null && Instance != null)
                    _InstancePropertyInfo = Instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                return _InstancePropertyInfo;
            }
        }

        PropertyInfo[] _InstancePropertyInfo;

        protected IDynamicMetaObjectProvider _dynamic;

        public DynamicTypeDescriptorWrapper(object instance)
        {
            _dynamic = new ExpandoObject();
            Initialize(instance);
        }

        public DynamicTypeDescriptorWrapper(IDynamicMetaObjectProvider dynamicObject)
        {
            _dynamic = dynamicObject;
            Initialize(this);
        }

        public DynamicTypeDescriptorWrapper()
        {
            _dynamic = new ExpandoObject();
            Initialize(this);
        }

        protected virtual void Initialize(object instance)
        {
            Instance = instance;
            if (instance != null)
                InstanceType = instance.GetType();
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

            if (InstanceType != null)
            {
                foreach (var propertyInfo in InstancePropertyInfo)
                {
                    props.Add(InstancePropertyDescriptionCollection[propertyInfo.Name]);
                }
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

        #region Add/Remove Attribute
        public void AddAttribute(string propertyName, Attribute attribute)
        {
            if (!ContainsProperty(propertyName))
            {
                throw new Exception("Property doesn't exist");
            }

            DynamicPropertyDescriptor property = null;
            if (properties.ContainsKey(propertyName))
            {
                property = properties[propertyName];
                properties.Remove(propertyName);
            }
            else
            {
                property = new DynamicPropertyDescriptor(new ExpandoObject(), propertyName);
            }

            var newPropertyWithAttribute = new DynamicPropertyDescriptor(property, new Attribute[] { attribute });
            properties.Add(propertyName, newPropertyWithAttribute);
        }

        public bool RemoveAttribute<TAttribute>(string propertyName) where TAttribute : Attribute
        {
            if (!ContainsProperty(propertyName))
            {
                throw new Exception("Property doesn't exist");
            }
            else if(InstancePropertyDescriptionCollection.ContainsKey(propertyName) && InstancePropertyDescriptionCollection[propertyName].Attributes.OfType<TAttribute>().Count() > 0)
            {
                throw new Exception("Can't remove attribute from strongly typed instance.");
            }

            if (properties.ContainsKey(propertyName))
            {
                DynamicPropertyDescriptor property = properties[propertyName];
                properties.Remove(propertyName);

                var attributesToCopy = new List<Attribute>();
                for (int i = 0; i < property.Attributes.Count; i++)
                {
                    if(property.Attributes[i].GetType() != typeof(TAttribute))
                    {
                        attributesToCopy.Add(property.Attributes[i]);
                    }
                }

                var newPropertyWithAttribute = new DynamicPropertyDescriptor(_dynamic, propertyName);
                foreach (var attribute in attributesToCopy)
                {
                    newPropertyWithAttribute = new DynamicPropertyDescriptor(newPropertyWithAttribute, new Attribute[] { attribute });
                }

                properties.Add(propertyName, newPropertyWithAttribute);
                return true;
            }
            return false;
        }

        public IEnumerable<TAttribute> GetAttribute<TAttribute>(string propertyName) where TAttribute : Attribute
        {
            if (!ContainsProperty(propertyName))
            {
                throw new Exception("Property doesn't exist");
            }

            if (InstancePropertyDescriptionCollection != null & InstancePropertyDescriptionCollection.ContainsKey(propertyName))
            {
                var property = GetProperties()[propertyName];
                var collection = property.Attributes.OfType<TAttribute>().ToList();

                if (properties.ContainsKey(propertyName))
                {
                    var dynamicProperty = properties[propertyName];
                    collection.AddRange(dynamicProperty.Attributes.OfType<TAttribute>());
                }

                return collection;
            }
            else
            {
                var property = GetProperties()[propertyName];
                return property.Attributes.OfType<TAttribute>();
            }
        }
        #endregion

        #region Contains Property
        public bool ContainsProperty(string property)
        {
            return GetProperties().Find(property, true) != null;
        }
        #endregion

        #region Add/Remove Properties
        public IDynamicBuilder Add(string property, object value)
        {
            if (!(_dynamic is IDictionary<string, object>))
            {
                throw new Exception("Can only add properties to Expando Objects");
            }
            else if (ContainsProperty(property))
            {
                throw new Exception("Already contains property");
            }

            var dict = _dynamic as IDictionary<string, object>;
            AddProperty(dict, property, value);

            return new DynamicBuilder(this, property);
        }

        private void AddProperty(IDictionary<string, object> dict, string property, object value)
        {
            dict.Add(property, value);
        }

        public bool Remove(string property)
        {
            if (!(_dynamic is IDictionary<string, object>))
            {
                throw new Exception("Can only remove properties from Expando Objects");
            }

            var dict = _dynamic as IDictionary<string, object>;
            return RemoveProperty(dict, property);
        }

        private bool RemoveProperty(IDictionary<string, object> dict, string property)
        {
            dict.Remove(property);
            return properties.Remove(property);
        }
        #endregion

        #region Dynamic Methods
        private object GetMember(string propertyName)
        {
            if (Instance != null && InstancePropertyDescriptionCollection.ContainsKey(propertyName))
            {
                return GetProperties().Find(propertyName, true).GetValue(Instance);
            }
            else
            {
                return GetProperties().Find(propertyName, true).GetValue(_dynamic);
            }
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

            if (Instance != null && InstancePropertyDescriptionCollection.ContainsKey(propertyName))
            {
                property.SetValue(Instance, value);
            }
            else
            {
                property.SetValue(_dynamic, value);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // first check to see if there's a native property to set
            if (ContainsProperty(binder.Name))
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
            var memberNames = meta.GetDynamicMemberNames().ToList();

            if (InstanceType != null)
            {
                foreach (var prop in this.InstancePropertyInfo)
                {
                    memberNames.Add(prop.Name);
                }
            }

            return memberNames;
        }
        #endregion
    }

    public class DynamicBuilder : IDynamicBuilder
    {
        private readonly DynamicTypeDescriptorWrapper _dynamic;
        private readonly string _propertyName;
        public DynamicBuilder(DynamicTypeDescriptorWrapper dynamic, string propertyName)
        {
            _dynamic = dynamic;
            _propertyName = propertyName;
        }

        public IDynamicBuilder AddAttribute(params Attribute[] attributes)
        {
            foreach (var attribute in attributes)
            {
                _dynamic.AddAttribute(_propertyName, attribute);
            }
            return this;
        }
    }

    public interface IDynamicBuilder
    {
        IDynamicBuilder AddAttribute(params Attribute[] attributes);
    }
}