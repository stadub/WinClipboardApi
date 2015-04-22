using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils
{
    public static class TypeHelpers
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

        public static bool TryChangeObjectType(Type destType, object sourceValue, out object value)
        {
            value = null;
            if (!destType.IsInstanceOfType(sourceValue))
            {
                if (sourceValue == null)
                    return true;

                try
                {
                    var convertedValue = Convert.ChangeType(sourceValue, destType);
                    value = convertedValue;
                    return true;
                }
                catch (InvalidCastException)
                {
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
                return false;
            }
            value = sourceValue;
            return true;
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
            object convertedValue = null;
            if (TryChangeObjectType(propertyType, value, out convertedValue))
            {
                property.SetValue(instance, convertedValue);
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

        public static PropertyInfo GetPropertyInfo<TClass, TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            return GetPropertyInfo((Expression)poperty);
        }

        internal static PropertyInfo GetPropertyInfo(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetPropertyInfo(((LambdaExpression)expression).Body);
                case ExpressionType.MemberAccess:
                    {
                        var ma = (MemberExpression)expression;
                        var prop = ma.Member as PropertyInfo;
                        return prop;
                    }
                default:
                    throw new ArgumentException("Only property expression is alowed", "expression");
            }
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
