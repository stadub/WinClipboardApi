using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils.TypeMapping.ValueResolvers.ServiceLocator
{
    public class LocatorRegistrationInfoResolver : ISourceMappingResolver
    {
        private readonly Utils.ServiceLocator locator;

        public LocatorRegistrationInfoResolver(Utils.ServiceLocator locator)
        {
            this.locator = locator;
            PropertyInjectionResolvers= new Dictionary<KeyValuePair<string, string>, KeyValuePair<string, Type>>();
        }

        public Dictionary<KeyValuePair<string, string>, KeyValuePair<string, Type>> PropertyInjectionResolvers { get; private set; }


        public bool IsMemberSuitable(IPropertyMappingInfo propInfo)
        {
            var propInfoKey=BuilderUtils.GetKey(propInfo);
            return PropertyInjectionResolvers.ContainsKey(propInfoKey);
        }

        public bool IsMemberSuitable(ParameterInfo mappingMember)
        {
            return false;
        }

        public ISourceInfo ResolveSourceValue(IPropertyMappingInfo propInfo, object source)
        {
            var propInfoKey=BuilderUtils.GetKey(propInfo);
            KeyValuePair<string, Type> valueType;
            if (PropertyInjectionResolvers.TryGetValue(propInfoKey, out valueType))
            {
                object value;
                if (locator.TryResolve(valueType.Value, valueType.Key, out value))
                {
                    return SourceInfo.Create(value);
                }
            }
            return null;
        }

        public ISourceInfo ResolveSourceValue(ParameterInfo mappingMember, object source)
        {
            return null;
        }


        public void AddInjectionResolver(PropertyInfo propInfo, Type type)
        {
            var key = BuilderUtils.GetKey(propInfo);
            PropertyInjectionResolvers.Add(key, new KeyValuePair<string, Type>(string.Empty,type));
        }

    }
}