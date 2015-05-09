using System;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers.ServiceLocator;

namespace Utils
{
    public class TypeMapper<TSource, TDest> : TypeMapperBase<TSource, TDest>
    {
        private readonly PropertyMappingInfo<TDest> propertyMappingInfo;
        private readonly LocatorRegistrationInfo<TDest> locatorMappingInfo;
        private readonly PropertyRegistrationInfo<TDest> registrationInfo;
        private ServiceLocator locator;

        public TypeMapper(ServiceLocator locator)
        {
            DestType = typeof(TDest);

            SourceType = typeof(TSource);
            propertyMappingInfo = new PropertyMappingInfo<TDest>();
            registrationInfo= new PropertyRegistrationInfo<TDest>();
            locatorMappingInfo = new LocatorRegistrationInfo<TDest>();
            this.locator = locator;
            LocatorInjectOnlyFlaggedProperties = true;

        }

        public TypeMapper():this(new ServiceLocator())
        {
        }

        public ILocatorRegistrationInfo<TDest> LocatorMappingInfo
        {
            get { return locatorMappingInfo; }
        }

        public IPropertyMappingInfo<TDest> PropertyMappingInfo 
        {
            get { return propertyMappingInfo; }
        }

        public IPropertyRegistrationInfo<TDest> RegistrationInfo
        {
            get { return registrationInfo; }
        }


        public bool LocatorInjectOnlyFlaggedProperties { get;set; }


        public override IOperationResult<TDest> TryMap(TSource source)
        {
            var mapper = CreateTypeBuilder();

            mapper.PropertyMappings = propertyMappingInfo.Mapping;

            var propertyResolver = new LocatorRegistrationInfoResolver(locator);

            locatorMappingInfo.PropertyInjectionResolvers
                .ForEach(_=>propertyResolver.PropertyInjectionResolvers.Add(_.Key,_.Value));

            mapper.RegisterSourceResolver(propertyResolver);

            var ctor = mapper.GetConstructor();

            mapper.CreateBuildingContext();
            mapper.InitBuildingContext();
            mapper.Context.Source = source;
            mapper.CreateInstance(ctor, DestType.FullName);
            mapper.CallInitMethods();

            mapper.InjectTypeProperties();

            return OperationResult<TDest>.Successful(mapper.Context.Instance);
        }

        protected virtual MappingTypeBuilder<TSource, TDest> CreateTypeBuilder()
        {
            if (DestType.IsGenericType || DestType.IsGenericType)
                throw new TypeNotSupportedException(DestType.FullName, "Generic types are not supported");

            var mapper = new MappingTypeBuilder<TSource, TDest>(locator, registrationInfo);
            return mapper;
        }

        public Type DestType { get; private set; }

        public Type SourceType { get; private set; }

    }


}
