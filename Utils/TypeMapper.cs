using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Utils.TypeMapping;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.TypeBuilders;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers;
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

    public class MappingTypeBuilder<TSource,TDest> : TypeBuilder<TDest>
    {
        public bool LocatorInjectOnlyFlaggedProperties { get; set; }
        public MappingTypeBuilder(ServiceLocator locator, PropertyRegistrationInfo<TDest> registrationInfo)
            : this(registrationInfo)
        {
            var injector = new LocatorValueInjector(locator)
            {
                InjectOnlyFlaggedProperties = LocatorInjectOnlyFlaggedProperties
            };
            InitLovcatorValueInjector(injector);
        }

        public MappingTypeBuilder(LocatorValueInjector injector, PropertyRegistrationInfo<TDest> registrationInfo)
            : this(registrationInfo)
        {
            InitLovcatorValueInjector(injector);
        }

        public MappingTypeBuilder(): this(new PropertyRegistrationInfo<TDest>())
        {
        }
        
        public MappingTypeBuilder(PropertyRegistrationInfo<TDest> registrationInfo): base(registrationInfo)
        {
            base.RegisterSourceResolver(new SourceAttributeResolver());
            base.RegisterSourceResolver(new SourcePropertyResolver());
            base.RegisterSourceResolver(new SourceTypeInjector());
        }


        private void InitLovcatorValueInjector(LocatorValueInjector injector)
        {
            base.RegisterSourceResolver(injector);
        }

        public IList<KeyValuePair<Expression, ITypeMapper>> PropertyMappings { get; set; }

        public new TypeMapperContext<TSource, TDest> Context
        {
            get { return (TypeMapperContext<TSource, TDest>) base.Context; }
            set { base.Context = value; }
        }

        public override void CreateBuildingContext()
        {
            var properyMappers= new Dictionary<PropertyInfo, ITypeMapper>();
            foreach (var propertyMapping in PropertyMappings)
            {
                var propInfo = TypeHelpers.GetPropertyInfo(propertyMapping.Key);
                properyMappers.Add(propInfo,propertyMapping.Value);
            }
            base.Context= new TypeMapperContext<TSource, TDest>(properyMappers);
        }


        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, PropertyInfo propertyInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, Context.Source);
        }

        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, ParameterInfo parameterInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(parameterInfo, Context.Source);
        }
    }

    public class TypeMapperContext<TSource, TDest> : TypeBuilerContext<TDest>
    {
        public TypeMapperContext(Dictionary<PropertyInfo, ITypeMapper> propertyMappers) : base(propertyMappers)
        {
        }

        public TSource Source { get; set; }
        
    }
}
