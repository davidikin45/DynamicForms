using System;
using System.Linq;
using System.Reflection;

namespace DynamicForms
{
    public static class ObjectExtensions
    {
        public static object GetPropValue(this object obj, string propName)
        {
            if (HasProperty(obj, propName))
            {
                return obj.GetType().GetProperties().First(p => p.Name.ToUpper() == propName.ToUpper()).GetValue(obj, null);
            }
            return null;
        }

        public static bool HasProperty(this object obj, string propName)
        {
            return obj.GetType().GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        public static bool HasProperty(this Type type, string propName)
        {
            return type.GetProperties().Any(p => p.Name.ToUpper() == propName.ToUpper());
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            if (HasField(obj, fieldName))
            {
                return obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
            }
            return null;
        }

        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        }

        public static bool HasField(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null;
        }
    }
}
