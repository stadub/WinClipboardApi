using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.ServiceLocatorInfo;

namespace Utils
{
    public interface ITypeMapper
    {
        object Map(object source);
        Type DestType { get; }
        Type SourceType { get; }
    }


    public class TypeMapper<TSource,TDest> : ITypeMapper
    {
        private readonly TypeBuilder baseBuilder;
        private readonly TypeBuilder mappingInfoStub;

        public TypeMapper():this(new TypeBuilderStub(typeof(TDest)))
        {
        }

        public TypeMapper(TypeBuilder baseBuilder)
        {
            this.baseBuilder = baseBuilder;
            DestType = typeof(TDest);

            mappingInfoStub = new TypeBuilderStub(DestType);
            MappingInfo = new LocatorRegistrationInfo<TDest>(mappingInfoStub);
            SourceType = typeof(TSource);
        }

        public TypeMapper(ServiceLocator locator): this(new LocatorTypeBuilder(locator, typeof(TDest)))
        {
            LocatorMappingInfo = new LocatorRegistrationInfo<TDest>(baseBuilder);
        }

        public IPropertyRegistrationInfo<TDest> MappingInfo { get; private set; }
        public ILocatorRegistrationInfo<TDest> LocatorMappingInfo { get; private set; }

        public TDest Map(object source)
        {
            if (DestType.IsGenericType || DestType.IsGenericType)
                throw new TypeNotSupportedException(DestType.FullName, "Generic types are not supported");

            var mapper = new MappingTypeBuilder(DestType, baseBuilder);

            mapper.PropertyValueResolvers.AddRange(mappingInfoStub.PropertyValueResolvers);
            mapper.IgnoreProperties.AddRange(mappingInfoStub.IgnoreProperties);
            mapper.PropertyInjections.AddRange(mappingInfoStub.PropertyInjections);

            var ctor = mapper.GetConstructor();

            TypeMaperContext context = (TypeMaperContext)mapper.CreateBuildingContext();
            context.Source = source;
            context.SourceType = source.GetType();
            mapper.CreateInstance(ctor, DestType.FullName, context);
            mapper.CallInitMethod(context);
            mapper.InjectTypeProperties(context);
            
            return (TDest)context.Instance;
        }

        public Type DestType { get; private set; }

        public Type SourceType { get; private set; }

        object ITypeMapper.Map(object source)
        {
            return Map(source);
        }
    }

    public class MappingTypeBuilder : TypeBuilder
    {
        private readonly TypeBuilder baseBuilder;

        public MappingTypeBuilder(Type destType): this(destType,new TypeBuilderStub(destType) )
        {
        }

        public MappingTypeBuilder(Type destType, TypeBuilder baseBuilder): base(destType)
        {
            this.baseBuilder = baseBuilder;
            var propMapAttributeSet=destType.GetProperties()
                .Select(prop => Tuple.Create(prop.GetCustomAttribute<MapSourcePropertyAttribute>(), prop));

            var propUseInitalizer = propMapAttributeSet.Where(x => x.Item1 != null && x.Item1.UseInitalizer != null);
            var propInitalizerInjection = propUseInitalizer.Select(x => new KeyValuePair<string, PropertyInfo>(x.Item1.UseInitalizer, x.Item2));

            base.PropertyInjections.AddRange(propInitalizerInjection);
        }

        public override TypeBuilerContext CreateBuildingContext()
        {
            return new TypeMaperContext(DestType);
        }

        protected internal override void InjectTypeProperties(TypeBuilerContext context)
        {
            base.InjectTypeProperties(context);

            var baseContext=baseBuilder.CreateBuildingContext();
            baseContext.Instance = context.Instance;
            context.ResolvedProperties.ForEach(baseContext.ResolvedProperties.Add);
            baseBuilder.InjectTypeProperties(baseContext);
        }


    }

    public class MapSourcePropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string UseInitalizer { get; set; }
    }

    public class TypeMaperContext : TypeBuilerContextStub
    {
        public Object Source { get; set; }
        public Type SourceType { get; set; }
        public TypeMaperContext(Type destType) : base(destType)
        {
            
        }
        public override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyInfo, out object value)
        {
            bool result = false;
            value = null;

            var srcPropMap = propertyInfo.GetCustomAttribute<MapSourcePropertyAttribute>();
            if (srcPropMap != null)
            {
                result = ResolvePropertyMarkedSourcePropertyAttribute(propertyInfo.Name, propertyInfo.PropertyType, srcPropMap, out value);
            }

            if (!result)
                result = TryMapProperty(propertyInfo.Name, propertyInfo.PropertyType, out value);

            if (!result)
                result = base.ResolvePublicNotIndexedProperty(propertyInfo, out value);

            return result;
        }

        public override bool ResolveParameter(ParameterInfo paramInfo, string methodName, out object value)
        {
            if (paramInfo.ParameterType.IsAssignableFrom(SourceType))
            {
                value = Source;
                return true;
            }
            return TryMapProperty(paramInfo.Name, paramInfo.ParameterType, out value) ||
                base.ResolveParameter(paramInfo,methodName, out value);
        }


        public override bool ResolvePropertyInjection(string propertyName, Type propertyType, string injectionName, out object value)
        {
            var propAttr = DestType.GetProperty(propertyName).GetCustomAttribute<MapSourcePropertyAttribute>();

            object propResolveValue = null;
            var result = ResolvePropertyMarkedSourcePropertyAttribute(propertyName, propertyType, propAttr, out propResolveValue);
            if (!result)
            {
                var prop = TryFindAppropriateProperty(propertyName, SourceType);
                if (prop != null)
                    result = TypeHelpers.TryChangeObjectType(propertyType, prop.GetValue(Source), out value);
            }
            if (result)
            {
                DestType.GetMethod(injectionName).Invoke(base.Instance, new[] {propResolveValue});
                value = null;
                return false;//TODO:Should be changed to InMethod property set
            }

            return base.ResolvePropertyInjection(propertyName, propertyType, injectionName, out value);
        }

        private static bool NameIsSame(PropertyInfo property, string name)
        {
            return NameIsSame(property.Name, name);
        }

        private static bool NameIsSame(string propName, string name)
        {
            return propName.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        protected static List<PropertyInfo> EnumerateSourceProperties(Type @type)
        {
            return @type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead)
                .ToList();
        }

        public virtual PropertyInfo TryFindAppropriateProperty(string name, Type @type)
        {

            foreach (PropertyInfo sourceProperty in EnumerateSourceProperties(@type))
            {
                if (NameIsSame(sourceProperty, name))
                    return sourceProperty;
            }
            return null;
        }

        public virtual bool TryMapProperty(string name, Type destType, out object value)
        {
            value = null;
            var prop = TryFindAppropriateProperty(name, SourceType);
            if (prop == null)
                return false;

            return TypeHelpers.TryChangeObjectType(destType, prop.GetValue(Source), out value);
        }

        private bool ResolvePropertyMarkedSourcePropertyAttribute(string propertyName, Type propertyType, MapSourcePropertyAttribute srcPropMap, out object value)
        {
            object propValue = null;
            bool result = false;

            if (srcPropMap.Name != null && srcPropMap.Path != null)
                throw new PropertyMappingException(DestType.FullName, propertyName, "Either Name or Path in the MapSourcePropertyAttribute should be set.");

            if (!string.IsNullOrWhiteSpace(srcPropMap.Name))
            {

                result = TryMapProperty(srcPropMap.Name, propertyType, out propValue);
            }
            if (!string.IsNullOrWhiteSpace(srcPropMap.Path))
            {
                var path = srcPropMap.Path.Split('.');

                PropertyInfo prop;
                propValue = Source;

                for (int i = 0; i < path.Length; i++)
                {
                    prop = TryFindAppropriateProperty(path[i], propValue.GetType());
                    if (prop == null)
                    {
                        propValue = null;
                        break;
                    }
                    propValue = prop.GetValue(propValue);
                    if (propValue == null) break;
                }

                if (propValue != null)
                {
                    result = TypeHelpers.TryChangeObjectType(propertyType, propValue, out propValue);
                }

            } //UsingInitalizer
            value = propValue;
            return result;
        }
    }
}
