using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Utils.TypeMapping.MappingInfo
{

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

    public class LocatorRegistrationInfo<TClass> : ILocatorRegistrationInfo<TClass>
    {
        public Dictionary<KeyValuePair<string, string>, KeyValuePair<string, Type>> PropertyInjectionResolvers { get; set; }

        public LocatorRegistrationInfo()
        {
            PropertyInjectionResolvers= new Dictionary<KeyValuePair<string, string>, KeyValuePair<string, Type>>();
        }

        public void InjectProperty<TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            InjectNamedProperty(poperty, string.Empty);
        }

        public void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> poperty,string reristeredName)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(poperty);
            var key = BuilderUtils.GetKey(propInfo);
            PropertyInjectionResolvers.Add(key, new KeyValuePair<string, Type>(reristeredName,propInfo.PropertyType));

        }
    }
}
