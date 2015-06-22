using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Utils.TypeMapping;
using Utils.TypeMapping.TypeMappers;

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

        public static ConstructorInfo TryGetConstructor(Type type)
        {
            //enumerating only public ctors
            var ctors = type.GetConstructors();

            //search for constructor marked as [UseConstructor]
            foreach (var ctor in ctors)
            {
                var attributes = ctor.GetCustomAttributes(typeof(UseConstructorAttribute), false);
                if (attributes.Any())
                    return ctor;
            }
            //try to find default constructor
            foreach (var ctor in ctors)
            {
                var args = ctor.GetParameters();
                if (args.Length == 0)
                    return ctor;
            }
            //try to use first public
            if (ctors.Length > 0)
                return ctors[0];
            return null;
        }

        public static ConstructorInfo GetConstructor(Type type)
        {
            var ctor = TryGetConstructor(type);
            if (ctor != null)
                return ctor;
            throw new ConstructorNotResolvedException(type.FullName);
        }


        public static void SetPropertyValue(PropertyInfo property, object instance, string value,ITypeMapper mapper=null)
        {
            if (mapper==null)
                mapper = new ConvertTypeMapper();
            Debug.Assert(property != null);
            if (property == null || instance == null | value == null)
                return;
            var propertyType = property.PropertyType;

            var convertionResult = mapper.Map(value, propertyType);
            if (convertionResult.Success)
            {
                property.SetValue(instance, convertionResult.Value);
            }
            //Throws MissingMethodException if corresponding constructor not found
            var propValue = Activator.CreateInstance(propertyType, value);
            property.SetValue(instance, propValue);
        }

        public static string GetPropertyValue(PropertyInfo property, object instance, ITypeMapper<object,string> mapper = null,IEnumerable<Attribute> attributes=null)
        {
            var propertyValue = property.GetValue(instance);
            var info = new SourceInfo(propertyValue)
            {
                Attributes = attributes.ToArray() ?? new Attribute[0]
            };

            if(mapper==null)
                mapper= new FormatedStringMapper<object>();

            if (mapper is ITypeInfoMapper)
            {
                var infoMapper = (ITypeInfoMapper)mapper;
                var result=infoMapper.TryMap(info);
                if (result.Success)
                    return result.Value.ToString();
            }

            IOperationResult<string> mapResult = mapper.TryMap(info);
            if (mapResult.Success) 
                return mapResult.Value;
            try
            {
                var convertedValue = Convert.ChangeType(propertyValue, typeof(string)).ToString();
                return convertedValue;
            }
            catch (InvalidCastException) { }
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

    public class NonSerializableAttribute : Attribute
    {
    }
}
