using System.Reflection;

namespace DynamicForms
{
    public static class ObjectExtensions
    {
       
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
