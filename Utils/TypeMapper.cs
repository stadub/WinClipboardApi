using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public class TypeMapper
    {
        public TResult MapTo<TResult>(object source)
        {
            var sourceType = source.GetType();
            var sourceProps=GetPublicNotIndexedProperties(sourceType);

            var destType = typeof(TResult);
            var destProps = GetPublicNotIndexedProperties(destType).ToLookup(info => info.Name);

            if(sourceType.IsGenericType || destType.IsGenericType)
                throw new TypeNotSupportedException(sourceType, "Generic types are not supported");

            var ctor = destType.GetConstructors();
            if(ctor.Length != 1 )
                throw new TypeNotSupportedException(sourceType, "Only default constructor is supported");

            var destObject=Activator.CreateInstance<TResult>();

            foreach (var propertyInfo in sourceProps)
            {
                if(destProps.Contains(propertyInfo.Name))
                {
                    var destProp = destProps[propertyInfo.Name].Single();

                    var sourceValue = propertyInfo.GetValue(source);
                    if (propertyInfo.PropertyType != destProp.PropertyType)
                    {
                        var convertedValue = Convert.ChangeType(sourceValue, destProp.PropertyType);
                        destProp.SetValue(destObject, convertedValue);
                    }
                    else
                        destProp.SetValue(destObject, sourceValue);
                }
            }
            return destObject;
        }

        private IEnumerable<PropertyInfo> GetPublicNotIndexedProperties(Type type)
        {
            //work only with public Not special and not Index properties
            IEnumerable<PropertyInfo> props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName && x.GetIndexParameters().Length == 0);
            return props;
        }
    }
}
