using System;
using System.Reflection;

namespace Utils
{
    class LocatorTypeBuilder : TypeBuilder
    {
        private readonly ServiceLocator serviceLocator;

        public LocatorTypeBuilder(ServiceLocator serviceLocator, Type destType, string registrationNam)
            : base(destType, registrationNam)
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

        protected override object ResolvePropertyInjection(Type propertyType, string name)
        {
            object value;
            serviceLocator.TryResolve(propertyType, name, out value);
            return value;
        }

        protected override object ResolvePropertyValueInjection(Type propertyType, string name)
        {
            object value;
            serviceLocator.TryResolve(propertyType, name, out value);
            return value;
        }

        protected override object ResolvePropertyNamedInstance(Type propertyType, string name)
        {
            object value;
            serviceLocator.TryResolve(propertyType, name, out value);
            return value;
        }
    }
}