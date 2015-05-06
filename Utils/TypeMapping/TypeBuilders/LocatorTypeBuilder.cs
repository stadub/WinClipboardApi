using System;
using System.Collections.Generic;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.ValueResolvers.ServiceLocator;

namespace Utils.TypeMapping.TypeBuilders
{
    class LocatorTypeBuilder<T> : TypeBuilder<T>
    {
        public Dictionary<KeyValuePair<string, string>, KeyValuePair<string, Type>> PropertyResolvers { get; private set; }

        public LocatorTypeBuilder(ServiceLocator serviceLocator):this(serviceLocator,new PropertyRegistrationInfo<T>())
        {
            
        }

        public LocatorTypeBuilder(ServiceLocator serviceLocator,PropertyRegistrationInfo<T> registrationInfo)
            : this(new LocatorRegistrationInfoResolver(serviceLocator), serviceLocator, registrationInfo)
        {
        }


        public LocatorTypeBuilder(LocatorRegistrationInfoResolver propertyResolver,
            ServiceLocator serviceLocator, PropertyRegistrationInfo<T> registrationInfo)
            : base(registrationInfo)
        {
            var injectionResolver = propertyResolver;
            this.PropertyResolvers = propertyResolver.PropertyInjectionResolvers;
            RegisterSourceResolver(injectionResolver);
            RegisterSourceResolver(new LocatorValueInjector(serviceLocator){InjectOnlyFlaggedProperties = true});
        }

        public override void CreateBuildingContext()
        {
            base.Context= new TypeBuilerContext<T>();
        }

        public void InitBuildingContext(TypeBuilerContext context)
        {
            
        }
    }

}