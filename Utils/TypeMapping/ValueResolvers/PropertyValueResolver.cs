using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers
{
    public class PropertyValueResolver : ISourceMappingResolver
    {
        Dictionary<KeyValuePair<string,string>,object> propertyInjections= new Dictionary<KeyValuePair<string, string>, object>();
        public void RegisterInjection<TClass,TProp>(Expression<Func<TClass, TProp>> poperty, TProp value)
        {
            var propInfo = TypeHelpers.GetPropertyInfo(poperty);
            propertyInjections.Add(BuilderUtils.GetKey(propInfo), value);
        }

        public bool IsMemberSuitable(PropertyInfo propertyInfo)
        {
            return propertyInjections.ContainsKey(BuilderUtils.GetKey(propertyInfo));
        }

        public ISourceInfo ResolveSourceValue(PropertyInfo propertyInfo, object source)
        {
            var key = BuilderUtils.GetKey(propertyInfo);
            object value;
            if (propertyInjections.TryGetValue(key, out value))
            {
                return SourceInfo.Create(value);
            }
            return null;

        }

        public ISourceInfo ResolveSourceValue(ParameterInfo propertyInfo, object source)
        {
            return null;
        }

        public bool IsMemberSuitable(ParameterInfo mappingMember)
        {
            return false;
        }
    }
}
