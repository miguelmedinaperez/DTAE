using System;
using System.ComponentModel;

namespace PRFramework.Core.Common
{
    public interface IDynamicObject
    {
        object GetValue(string propertyName);
        bool SetValue(string propertyName, object propertyValue);
    }
    
    public static class ObjectPropertyHelper
    {
        public static bool SetPropertyValue(object target, string propertyName, object newValue)
        {
            var propertyInfo = target.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                object value;
                bool success = true;
                object[] typeConverterAttr =
                    propertyInfo.PropertyType.GetCustomAttributes(typeof (TypeConverterAttribute), true);
                if (typeConverterAttr.Length > 0)
                {
                    string converterTypeName = ((TypeConverterAttribute) typeConverterAttr[0]).ConverterTypeName;
                    TypeConverter converter = (TypeConverter) Activator.CreateInstance(Type.GetType(converterTypeName));
                    value = converter.ConvertFrom(newValue);
                }
                else if (propertyInfo.PropertyType.IsEnum)
                    value = Enum.Parse(propertyInfo.PropertyType, newValue.ToString());
                else if (propertyInfo.PropertyType.IsValueType && newValue == null)
                    return false;
                else if (propertyInfo.PropertyType == typeof (int))
                {
                    int parsed;
                    success = int.TryParse(newValue.ToString(), out parsed);
                    value = parsed;
                }
                else if (propertyInfo.PropertyType == typeof (double))
                {
                    double parsed;
                    success = double.TryParse(newValue.ToString(), out parsed);
                    value = parsed;
                }
                else
                    try
                    {
                        value = Convert.ChangeType(newValue, propertyInfo.PropertyType);
                    }
                    catch
                    {
                        return false;
                    }
                if (success)
                    propertyInfo.SetValue(target, value, null);
                return success;
            }
            else if (target is IDynamicObject)
                return (target as IDynamicObject).SetValue(propertyName, newValue);
            return false;
        }


        public static bool TryGetPropertyValue(object target, string propertyName, out object value)
        {
            var propertyInfo = target.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(target, null);
                return true;
            }
            value = null;
            return false;
        }

        public static object GetPropertyValue(object target, string propertyName)
        {
            var propertyInfo = target.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
                return propertyInfo.GetValue(target, null);
            if (target is IDynamicObject)
                return (target as IDynamicObject).GetValue(propertyName);
            throw new Exception(string.Format("Unknown parameter {0} in class {1}", propertyName, target.GetType().Name));
        }

        public static string GetPropertyValue(object target, string propertyName, string formatString)
        {
            object value = GetPropertyValue(target, propertyName);
            return string.Format(formatString, value);
        }
    }
}