using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;


namespace Utils
{
    public class TypeHelpers
    {
        public static object GetDefault(Type type)
        {
            //http://stackoverflow.com/questions/325426/programmatic-equivalent-of-defaulttype
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static void SetPropertyValue(PropertyInfo property, object instance, string value)
        {
            Debug.Assert(property != null);
            if (property == null || instance == null | value == null)
                return;
            var propertyType = property.PropertyType;
            var stringType = typeof(string);
            if (propertyType == stringType)
            {
                property.SetValue(instance, value);
                return;
            }
            try
            {
                var convertedValue = Convert.ChangeType(value, propertyType);
                property.SetValue(instance, convertedValue);
                return;
            }
            catch (InvalidCastException)
            {
            }
            //Throws MissingMethodException if corresponding constructor not found
            var propValue = Activator.CreateInstance(propertyType, value);
            property.SetValue(instance, propValue);
        }

        public static string GetPropertyValue(PropertyInfo property, object instance)
        {
            var propertyValue = property.GetValue(instance);

            var formated = property.GetCustomAttribute<FormatedAttribute>();
            if (formated != null)
            {
                Debug.Assert(formated.Format != null);
                var propertyConverter = (IFormattable)propertyValue;
                var formatedString = propertyConverter.ToString(formated.Format, CultureInfo.InvariantCulture);
                return formatedString;
            }
            try
            {
                var convertedValue = Convert.ChangeType(propertyValue, typeof(string));
                return (string)convertedValue;
            }
            catch (InvalidCastException)
            {
            }

            return propertyValue.ToString();

        }
    }
    public class FormatedAttribute : Attribute
    {
        public FormatedAttribute(string format)
        {
            this.Format = format;
        }

        public string Format { get; set; }
    }
    public class FormatedNumericAttribute : FormatedAttribute
    {
        public FormatedNumericAttribute()
            : base("D")
        {
        }
        public FormatedNumericAttribute(string format)
            : base(format)
        {
        }
    }
}
