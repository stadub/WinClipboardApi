using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Utils.TypeMapping;

namespace Utils.ServiceLocatorInfo
{
    public interface IPropertyRegistrationInfo<TClass>
    {
        void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value);
        void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> poperty);
    }
    
    public interface IPropertyMappingInfo<TClass>
    {
        void MapProperty<TSourceProp,TProp>(Expression<Func<TClass,TSourceProp, TProp>> poperty, ITypeMapper<TSourceProp, TProp> mapper);
    }

    public class PropertyMappingInfo<TClass> : IPropertyMappingInfo<TClass>
    {
        public PropertyMappingInfo()
        {
            this.Mapping = new List<KeyValuePair<Expression, ITypeMapper>>();
        }

        public IList<KeyValuePair<Expression, ITypeMapper>> Mapping { get; private set; }
        public void MapProperty<TSourceProp, TProp>(Expression<Func<TClass, TSourceProp, TProp>> poperty, ITypeMapper<TSourceProp, TProp> mapper)
        {
            Mapping.Add(new KeyValuePair<Expression, ITypeMapper>(poperty, mapper));
        }

    }

    public interface ILocatorRegistrationInfo<TClass> : IPropertyRegistrationInfo<TClass>
    {
        void InjectProperty<TProp>(Expression<Func<TClass, TProp>> poperty);
        void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> poperty,string reristeredName);
    }

    public class LocatorRegistrationInfo<TClass> : ILocatorRegistrationInfo<TClass>
    {
        private readonly TypeBuilder type;

        internal LocatorRegistrationInfo(TypeBuilder type)
        {
            this.type = type;
        }

        public void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(poperty);
            type.PropertyValueResolvers.Add(new KeyValuePair<PropertyInfo, object>(propInfo, value));
        }
#if PropertyInjectionResolvers
        public void InjectPropertyType<TBindType>(Expression<Func<TClass, object>> expression)
        {
            var propInfo = GetPropertyInfo(expression);

            type.PropertyInjectionResolvers.Add(new KeyValuePair<PropertyInfo, Type>(propInfo, typeof(TBindType)));
        }
#endif
        public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            InjectNamedProperty(poperty, string.Empty);
        }

        public void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> poperty,string reristeredName)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(poperty);
            type.PropertyInjections.Add(new KeyValuePair<string, PropertyInfo>(reristeredName, propInfo));
        }

        public void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> expression)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(expression);
            type.IgnoreProperties.Add(propInfo);
        }

    }
    
}
