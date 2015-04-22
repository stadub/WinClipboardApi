using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Utils.ServiceLocatorInfo;

namespace Utils
{
    public interface ITypeMapper
    {
        object Map(object source);
    }

    public interface ITypeMapper<in TSource, out TDest> : ITypeMapper
    {
        TDest Map(TSource source);
    }


    public class MappingFunc<TSource, TDest> : ITypeMapper<TSource, TDest>
    {
        private readonly Func<TSource,TDest> mapper;

        public MappingFunc(Func<TSource,TDest> mapper)
        {
            this.mapper = mapper;
        }

        object ITypeMapper.Map(object source)
        {
            return Map((TSource)source);
        }

        public TDest Map(TSource source)
        {
            return mapper(source);
        }
    }

    public class TypeMapper<TSource, TDest> : ITypeMapper<TSource, TDest>
    {
        private readonly TypeBuilder baseBuilder;
        private readonly TypeBuilder mappingInfoStub;
        private readonly PropertyMappingInfo<TDest> propertyMappingInfo;

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
            propertyMappingInfo = new PropertyMappingInfo<TDest>();
        }

        public TypeMapper(ServiceLocator locator): this(new LocatorTypeBuilder(locator, typeof(TDest)))
        {
            LocatorMappingInfo = new LocatorRegistrationInfo<TDest>(baseBuilder);
        }

        public IPropertyRegistrationInfo<TDest> MappingInfo { get; private set; }
        public ILocatorRegistrationInfo<TDest> LocatorMappingInfo { get; private set; }

        public IPropertyMappingInfo<TDest> PropertyMappingInfo
        {
            get { return propertyMappingInfo; }
        }


        public TDest Map(TSource source)
        {
            if (DestType.IsGenericType || DestType.IsGenericType)
                throw new TypeNotSupportedException(DestType.FullName, "Generic types are not supported");

            var mapper = new MappingTypeBuilder(DestType, baseBuilder);
            mapper.PropertyMappings = propertyMappingInfo.Mapping;
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
            return Map((TSource)source);
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
            var propMapAttributeSet=destType.GetProperties();

            foreach (var prop in propMapAttributeSet)
            {
                var mapAttribute = prop.GetCustomAttribute<MapSourcePropertyAttribute>();
                if(mapAttribute==null)
                    continue;

                base.PropertyInjections.Add(new KeyValuePair<string, PropertyInfo>(prop.Name,prop));
            }
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

            return new TypeMaperContext(DestType,properyMappers);
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
        public TypeMaperContext(Type destType, Dictionary<PropertyInfo, ITypeMapper> properyMappers) : base(destType,properyMappers)
        {
            
        }
        public override MappingResult ResolvePublicNotIndexedProperty(PropertyInfo propertyInfo)
        {
            MappingResult result = MappingResult.NotResolved;

            var srcPropMap = propertyInfo.GetCustomAttribute<MapSourcePropertyAttribute>();
            if (srcPropMap != null)
            {
                result = ResolvePropertyMarkedSourcePropertyAttribute(propertyInfo, srcPropMap);
            }

            if (result!=MappingResult.Resolved)
                result = TryMapProperty(propertyInfo);

            if (result != MappingResult.Resolved)
                result = base.ResolvePublicNotIndexedProperty(propertyInfo);

            return result;
        }

        public override bool ResolveParameter(ParameterInfo paramInfo, string methodName, out object value)
        {
            if (paramInfo.ParameterType.IsAssignableFrom(SourceType))
            {
                value = Source;
                return true;
            }

            value = null;
            bool result = false;
            var prop = TryFindAppropriateProperty(paramInfo.Name, SourceType);
            if (prop != null)
            {
                result= TypeHelpers.TryChangeObjectType(paramInfo.ParameterType, prop.GetValue(Source), out value);
                
            }

            return result || base.ResolveParameter(paramInfo,methodName, out value);
        }


        public override MappingResult ResolvePropertyInjection(PropertyInfo propInfo, string injectionName)
        {
            var propertyName = propInfo.Name;

            var propAttr = DestType.GetProperty(propertyName).GetCustomAttribute<MapSourcePropertyAttribute>();

            var result= ResolvePropertyMarkedSourcePropertyAttribute(propInfo, propAttr);
            
            return result==MappingResult.Resolved ? result : base.ResolvePropertyInjection(propInfo, injectionName);
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

        public virtual MappingResult TryMapProperty(PropertyInfo destPropertyInfo)
        {

            var srcProp = TryFindAppropriateProperty(destPropertyInfo.Name, SourceType);

            if(srcProp==null)
                return MappingResult.NotResolved;
            var value = srcProp.GetValue(Source);

            return MapProperty(destPropertyInfo, value);
        }

        private MappingResult ResolvePropertyMarkedSourcePropertyAttribute(PropertyInfo propInfo, MapSourcePropertyAttribute srcPropMap)
        {
            object propValue = null;
            MappingResult result = MappingResult.NotResolved;

            if (srcPropMap.Name != null && srcPropMap.Path != null)
                throw new PropertyMappingException(DestType.FullName,propInfo.Name, "Either Name or Path in the MapSourcePropertyAttribute should be set.");

            if (!string.IsNullOrWhiteSpace(srcPropMap.Name))
            {
                var srcProp = TryFindAppropriateProperty(srcPropMap.Name, SourceType);

                var value = srcProp.GetValue(Source);

                return MapProperty(propInfo, value);

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
            }

            if (propValue == null)
            {
                var sourceProp = TryFindAppropriateProperty(propInfo.Name, SourceType);
                propValue = sourceProp.GetValue(Source);
            }

            if (propValue == null)
                return MappingResult.NotResolved;
            if (!string.IsNullOrWhiteSpace(srcPropMap.UseInitalizer))
            {
                try
                {
                    DestType.GetMethod(srcPropMap.UseInitalizer).Invoke(base.Instance, new[] {propValue});
                    return MappingResult.Resolved;
                }
                catch (AmbiguousMatchException){}
                catch (TargetException){}
                catch (ArgumentException){}
                catch (TargetInvocationException ){}
                catch (TargetParameterCountException ){}
                catch (MethodAccessException ){}
                catch (InvalidOperationException ){}
                catch (NotSupportedException) { }
                return MappingResult.NotResolved;
            }

            if(TypeHelpers.TryChangeObjectType(propInfo.PropertyType, propValue, out propValue)){
                return MapProperty(propInfo, propValue);
            }
            return MappingResult.NotResolved;
        }
    }
}
