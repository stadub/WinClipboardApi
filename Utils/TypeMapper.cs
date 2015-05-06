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

            var context = (TypeMapperContext<TSource, TDest>)mapper.CreateBuildingContext();
            mapper.InitBuildingContext(context);
            context.Source = source;
            mapper.CreateInstance(ctor, DestType.FullName, context);
            mapper.CallInitMethods(context);

            mapper.InjectTypeProperties(context);

            return OperationResult<TDest>.Successful(context.Instance);
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

        public override TypeBuilerContext CreateBuildingContext()
        {
            var properyMappers= new Dictionary<PropertyInfo, ITypeMapper>();
            foreach (var propertyMapping in PropertyMappings)
            {
                var propInfo = TypeHelpers.GetPropertyInfo(propertyMapping.Key);
                properyMappers.Add(propInfo,propertyMapping.Value);
            }

            return new TypeMapperContext<TSource, TDest>(properyMappers);
        }


        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, PropertyInfo propertyInfo, TypeBuilerContext context)
        {
            var mapperContext =(TypeMapperContext<TSource, TDest>)context;
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, mapperContext.Source);
        }

        protected override OperationResult GetValue(ISourceMappingResolver sourceMappingResolver, ParameterInfo parameterInfo, TypeBuilerContext context)
        {
            var mapperContext =(TypeMapperContext<TSource, TDest>)context;
            return sourceMappingResolver.ResolveSourceValue(parameterInfo, mapperContext.Source);
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
