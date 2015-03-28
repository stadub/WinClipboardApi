using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils
{
    class LocatorTypeBuilder : TypeBuilder
    {
        private readonly ServiceLocator serviceLocator;
        public LocatorTypeBuilder(ServiceLocator serviceLocator, Type destType)
            : base(destType)
        {
            this.serviceLocator = serviceLocator;
        }

        protected override bool ResolveParameter(ParameterInfo paramInfo, out object value)
        {
            var parametrType = paramInfo.ParameterType;
            var name = string.Empty;
            var attribute = paramInfo.GetCustomAttribute<InjectInstanceAttribute>();
            if (attribute != null) name = attribute.Name;
            return serviceLocator.TryResolve(parametrType, name, out value);
        }

        //executed only when PropertyInjectionResolvers compilation option is defined
        protected override bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value)
        {
            throw new NotImplementedException();
        }

        protected override bool ResolvePropertyInjection(Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }

        protected override bool ResolvePropertyValueInjection(Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }

        protected override bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }


        protected override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value)
        {
            value = null;
            return false;
        }

        protected override Dictionary<PropertyInfo, object> ResolvePropertiesCustom(List<PropertyInfo> resolvedProperties)
        {
            return new Dictionary<PropertyInfo, object>();
        }
    }
}