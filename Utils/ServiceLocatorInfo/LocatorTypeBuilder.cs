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


        public override TypeBuilerContext CreateBuildingContext()
        {
            return new LocatorTypeBuilderContext(serviceLocator,DestType);
        }
    }

    class LocatorTypeBuilderContext : TypeBuilerContext
    {
        private readonly ServiceLocator serviceLocator;
        public LocatorTypeBuilderContext(ServiceLocator serviceLocator, Type destType)
            : base(destType)
        {
            this.serviceLocator = serviceLocator;
        }

        public override bool ResolveParameter(ParameterInfo paramInfo, string methodName, out object value)
        {
            var parametrType = paramInfo.ParameterType;
            var name = string.Empty;
            var attribute = paramInfo.GetCustomAttribute<InjectInstanceAttribute>();
            if (attribute != null) name = attribute.Name;
            return serviceLocator.TryResolve(parametrType, name, out value);
        }

        //executed only when PropertyInjectionResolvers compilation option is defined
        public override bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value)
        {
            throw new NotImplementedException();
        }

        public override bool ResolvePropertyInjection(string propertyName, Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }

        public override bool ResolvePropertyValueInjection(Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }

        public override bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value)
        {
            return serviceLocator.TryResolve(propertyType, name, out value);
        }


        public override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value)
        {
            value = null;
            return false;
        }

        public override Dictionary<PropertyInfo, object> ResolvePropertiesCustom(IList<PropertyInfo> resolvedProperties)
        {
            return new Dictionary<PropertyInfo, object>();
        }
    }
}