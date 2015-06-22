using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Utils.TypeMapping;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.TypeBuilders;
using Utils.TypeMapping.ValueResolvers;
using Utils.TypeMapping.ValueResolvers.ServiceLocator;

namespace Utils
{
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

        protected override ISourceInfo GetMappingData(ISourceMappingResolver sourceMappingResolver, IPropertyMappingInfo propertyInfo)
        {
            var sourceValue = sourceMappingResolver.ResolveSourceValue(propertyInfo, Context.Source);
            return sourceMappingResolver.ResolveSourceValue(propertyInfo, Context.Source);
        }

        protected override ISourceInfo GetValue(ISourceMappingResolver sourceMappingResolver, ParameterInfo parameterInfo)
        {
            return sourceMappingResolver.ResolveSourceValue(parameterInfo, Context.Source);
        }
    }

    public class TypeMapperContext<TSource, TDest> : TypeBuilerContext<TDest>
    {
        public TypeMapperContext(Dictionary<PropertyInfo, ITypeMapper> propertyMappers)
            : base(propertyMappers)
        {
        }
        public TypeMapperContext(): base()
        {
        }

        public TSource Source { get; set; }

        public Type SourceType
        {
            get
            {
                return Source==null ? null : Source.GetType();
            }
        }

    }
}