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
            : base(destType, new Dictionary<PropertyInfo, ITypeMapper>())
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
        public override MappingResult ResolvePropertyInjectionByResolver(PropertyInfo propInfo, string name)
        {
            throw new NotImplementedException();
        }

        public override MappingResult ResolvePropertyInjection(PropertyInfo propInfo, string injectionName)
        {

            object value;
            if (serviceLocator.TryResolve(propInfo.PropertyType, injectionName, out value))
            {
                return MapProperty(propInfo, value);
            }
            return MappingResult.NotResolved;
        }

        public override MappingResult ResolvePropertyValueInjection(PropertyInfo propInfo, string name)
        {

            object value;
            if (serviceLocator.TryResolve(propInfo.PropertyType, name, out value))
            {
                return MapProperty(propInfo, value);
            }
            return MappingResult.NotResolved;
        }

        public override MappingResult ResolvePropertyNamedInstance(PropertyInfo propertyInfo, string name)
        {
            
            object value;
            if (serviceLocator.TryResolve(propertyInfo.PropertyType, name, out value))
            {
                return MapProperty(propertyInfo, value);
            }
            return MappingResult.NotResolved;
        }


        public override MappingResult ResolvePublicNotIndexedProperty(PropertyInfo propertyType)
        {
            return MappingResult.NotResolved;
        }

        public override Dictionary<PropertyInfo, object> ResolvePropertiesCustom(IList<PropertyInfo> resolvedProperties)
        {
            return new Dictionary<PropertyInfo, object>();
        }
    }
}