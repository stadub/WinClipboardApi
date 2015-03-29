using System;
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

            var mapper = new MappingTypeBuilder(source, DestType, baseBuilder);

            mapper.PropertyValueResolvers.AddRange(mappingInfoStub.PropertyValueResolvers);
            mapper.IgnoreProperties.AddRange(mappingInfoStub.IgnoreProperties);
            mapper.PropertyInjections.AddRange(mappingInfoStub.PropertyInjections);

            var ctor = mapper.GetConstructor();

            var destObject = mapper.CreateInstance(ctor, DestType);
            mapper.InjectTypeProperties(destObject);

            return (TDest) destObject;
        }

        public Type DestType { get; private set; }

        public Type SourceType { get; private set; }

        object ITypeMapper.Map(object source)
        {
            return Map(source);
        }
    }

    public class MappingTypeBuilder : TypeBuilderProxy
    {
        private readonly object source;
        private readonly Type sourceType;


        public MappingTypeBuilder(object source, Type destType): this(source,destType,new TypeBuilderStub(destType) )
        {
        }

        public MappingTypeBuilder(object source, Type destType, TypeBuilder baseBuilder): base(destType, baseBuilder)
        {
            this.source = source;
            this.sourceType = source.GetType();
        }

        private static bool NameIsSame(PropertyInfo property,string name)
        {
            return NameIsSame(property.Name,name);
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
                if (NameIsSame(sourceProperty,name))
                    return sourceProperty;
            }
            return null;
        }

        public virtual bool TryMapProperty(string name, Type type, out object value)
        {
            value = null;
            var prop = TryFindAppropriateProperty(name, sourceType);
            if (prop == null)
                return false;

            var sourcePropValue = prop.GetValue(source);
            if (prop.PropertyType != type)
            {
                if (sourcePropValue == null)
                    return true;

                try
                {
                    var convertedValue = Convert.ChangeType(sourcePropValue, type);
                    value = convertedValue;
                    return true;
                }
                catch (InvalidCastException) { }
                catch (FormatException) { }
                catch (OverflowException)
                {
                }
                return false;
            }
            value = sourcePropValue;
            return true;
        }

        protected override bool ResolvePropertyInjection(Type propertyType, string name, out object value)
        {
            foreach (var sourceProperty in EnumerateSourceProperties(sourceType))
            {
                if (NameIsSame(sourceProperty, name) && propertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    value = sourceProperty.GetValue(source);
                    return true;
                } 
            }

            return base.ResolvePropertyInjection(propertyType, name, out value);
        }

        private bool ResolvePropertyMarkedSourcePropertyAttribute(PropertyInfo propertyInfo, SourcePropertyAttribute srcPropMap, out object value)
        {
            if ((srcPropMap.Name != null && srcPropMap.Path != null) || (srcPropMap.Name == null && srcPropMap.Path == null))
                throw new PropertyMappingException(DestType.FullName, propertyInfo.Name, "Either Name or Path in the SourcePropertyAttribute should be set.");
            if (!string.IsNullOrWhiteSpace(srcPropMap.Name))
            {
                return TryMapProperty(srcPropMap.Name, propertyInfo.PropertyType, out value);
            }

            if (!string.IsNullOrWhiteSpace(srcPropMap.Path))
            {
                var path = srcPropMap.Path.Split('.');

                PropertyInfo prop;
                object propValue = source;

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
                    value = propValue;
                    return true;
                }

            }
            value = null;
            return false;
        }

        protected override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyInfo, out object value)
        {
            bool result = false;
            value = null;

            var srcPropMap=propertyInfo.GetCustomAttribute<SourcePropertyAttribute>();
            if (srcPropMap != null)
            {
                result=ResolvePropertyMarkedSourcePropertyAttribute(propertyInfo, srcPropMap, out value);
            }

            if (!result)
                result = TryMapProperty(propertyInfo.Name, propertyInfo.PropertyType, out value);

            if(!result)
                result= base.ResolvePublicNotIndexedProperty(propertyInfo, out value);
            
            return result;
        }

        protected override bool ResolveParameter(ParameterInfo paramInfo, out object value)
        {
            if (paramInfo.ParameterType.IsAssignableFrom(sourceType))
            {
                value = source;
                return true;
            }
            return TryMapProperty(paramInfo.Name, paramInfo.ParameterType, out value) ||
                base.ResolveParameter(paramInfo, out value);
        }


    }

    public class SourcePropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
