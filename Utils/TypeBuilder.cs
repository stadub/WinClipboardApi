using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using Utils.TypeMapping;
using Utils.TypeMapping.TypeMappers;
using Utils.TypeMapping.ValueResolvers;

namespace Utils
{
    public interface IPropertyMapper
    {
        bool MapPropery(PropertyInfo property,object instance);
    }


    public abstract class TypeBuilder
    {
        public const string CtorParamName = "[ctor]";

        protected TypeBuilder(Type destType)
        {
            PropertyInjectionResolvers = new List<KeyValuePair<PropertyInfo, Type>>();
            PropertyValueResolvers = new List<KeyValuePair<PropertyInfo, object>>();
            PropertyInjections = new List<KeyValuePair<string, PropertyInfo>>();
            DestType = destType;
            IgnoreProperties= new List<PropertyInfo>();
            InitMethods=new List<MethodInfo>();
        }

        public List<KeyValuePair<string, PropertyInfo>> PropertyInjections { get; private set; }
        public Type DestType { get; private set; }

        public List<KeyValuePair<PropertyInfo,Type>> PropertyInjectionResolvers { get; private set; }
        public List<KeyValuePair<PropertyInfo,object>> PropertyValueResolvers { get; private set; }
        public List<PropertyInfo> IgnoreProperties { get; private set; }

        public List<MethodInfo> InitMethods{ get; private set; }


        public ConstructorInfo TryGetConstructor()
        {
            //enumerating only public ctors
            var ctors = DestType.GetConstructors();

            //search for constructor marked as [UseConstructor]
            foreach (var ctor in ctors)
            {
                var attributes = ctor.GetCustomAttributes(typeof(UseConstructorAttribute), false);
                if (attributes.Any())
                    return ctor;
            }
            //try to find default constructor
            foreach (var ctor in ctors)
            {
                var args = ctor.GetParameters();
                if (args.Length == 0)
                    return ctor;
            }
            //try to use first public
            if (ctors.Length > 0)
                return ctors[0];
            return null;
        }

        public ConstructorInfo GetConstructor()
        {
            var ctor = TryGetConstructor();
            if (ctor!=null)
                return ctor;
            throw new ConstructorNotResolvedException(DestType.FullName);
        }

        public void CreateInstance(ConstructorInfo ctor, string destTypeName, TypeBuilerContext context)
        {
            //resolving constructor parametrs
            var paramsDef = ctor.GetParameters();
            var paramValues = ResolvePramValues(paramsDef, CtorParamName, destTypeName, context);
            context.Instance = ctor.Invoke(paramValues.ToArray());
        }

        public object[] ResolvePramValues(IList<ParameterInfo> paramsDef, string methodName,string destTypeName, TypeBuilerContext context)
        {
            if (paramsDef.Count == 0)
                context.Instance = Activator.CreateInstance(context.DestType);

            var paramValues = new List<object>();
            foreach (ParameterInfo paramDef in paramsDef)
            {
                var parametrType = paramDef.ParameterType;
                if (paramDef.IsOut || parametrType.IsByRef)
                {
                    throw new TypeNotSupportedException(destTypeName, "Constructors/Methods with Out and Ref Attributes are not supported");
                }

                object paramValue = null;
                bool paramValueInjected;

                //optional parametr
                paramValueInjected = SetOptionalParameterValue(paramDef, parametrType, out paramValue);

                if (!paramValueInjected)
                {
                    //Inject parametr value from Inject Attribute
                    paramValueInjected = InjectParameterValue(paramDef, out paramValue, context);
                }

                if (!paramValueInjected)
                {
                    paramValueInjected = context.ResolveParameter(paramDef,methodName, out paramValue);
                }
                if (!paramValueInjected)
                    throw new TypeInitalationException(DestType.FullName, string.Format("Property {0} doesn't resolved", paramDef.Name));
                paramValues.Add(paramValue);
            }
            return paramValues.ToArray();
        }

        protected virtual bool SetOptionalParameterValue(ParameterInfo paramInfo, Type parametrType, out object value)
        {
            //optional parametr - defaut value or default(T)
            if (paramInfo.IsOptional)
            {
                value = paramInfo.HasDefaultValue ? paramInfo.DefaultValue : TypeHelpers.GetDefault(parametrType);
                return true;
            }
            value = null;
            return false;
        }

        protected virtual bool InjectParameterValue(ParameterInfo paramInfo, out object value, TypeBuilerContext context)
        {
            value = null;
            var parametrType = paramInfo.ParameterType;

            var valueInjector = new ValueInjector();

            if (valueInjector.IsMemberSuitable(paramInfo))
            {
                var source = valueInjector.ResolveSourceValue(paramInfo);
                if (source.Success)
                {
                    var convertedType = Convert.ChangeType(source.Value, parametrType);
                    value = convertedType;
                    return true;
                }
            }

            var inject = paramInfo.GetCustomAttribute<ShoudlInjectAttribute>();
            if (inject != null)
            {
                context.ResolveParameter(paramInfo, string.Empty, out value);
                return true;
            }

            return false;
        }





        protected static bool FilterInjectNamedInstance(CustomAttributeData field)
        {
            //declare property/ctor parametr filter signature
            return field.AttributeType.IsAssignableFrom(typeof(InjectInstanceAttribute));
        }

        protected static IEnumerable<PropertyInfo> GetPublicNotIndexedProperties(Type type)
        {
            //work only with public Not special and not Index properties
            IEnumerable<PropertyInfo> props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName && x.GetIndexParameters().Length == 0);
            return props;
        }

        public abstract TypeBuilerContext CreateBuildingContext();

        /// <summary>
        /// Resolves type properties by invoking for type peoperties methods sequence:
        /// 1) In case when <see cref="Utils"/> compilled with defined derectied "PropertyInjectionResolvers" 
        /// resolve properties listed in <see cref="PropertyInjectionResolvers"/> collection
        /// by invoking <see cref="ResolvePropertyInjectionByResolver"/> for each of them
        /// 2)Invking <see cref="ResolvePropertyInjection"/> for each property from <see cref="PropertyInjections"/> list.
        /// 3)Resolve value for properties marked with <see cref="InjectValueAttribute"/>
        /// ether by setting value defined in attribute or <see cref="ResolvePropertyValueInjection"/> method invocation.
        /// 4)Inject value for properties marked with <see cref="InjectInstanceAttribute"/>
        /// via <see cref="ResolvePropertyNamedInstance"/> method invocation.
        /// 5)<see cref="ResolvePublicNotIndexedProperty"/> method invoked for all public properties with no index that wherent resolved in previouse steps.
        /// 6)Execute <see cref="ResolvePropertiesCustom"/> with passing allready resolved properties list as argument
        /// </summary>
        /// <remarks>All the properties that marked as Ignored by adding to <see cref="IgnoreProperties"/> list will be ignored.</remarks>
        /// <param name="instance"></param>
        internal protected virtual void InjectTypeProperties(TypeBuilerContext context)
        {
            var resolvedProperties = context.ResolvedProperties;
            var instance = context.Instance;
            IgnoreProperties.ForEach(resolvedProperties.Add);
            //Inject registered properties values
            foreach (var prop in PropertyValueResolvers)
            {
                prop.Key.SetValue(instance, prop.Value);
                context.ResolvedProperties.Add(prop.Key);
            }

#if PropertyInjectionResolvers
            //Inject registered property injectction resolvers
            foreach (var prop in PropertyInjectionResolvers)
            {
                if(resolvedProperties.Contains(prop.Key))
                    continue;
                object propValue;
                context.ResolvePropertyInjectionByResolver(prop.Key, string.Empty);
            }
#endif
            //Inject registered property injections
            foreach (var prop in PropertyInjections)
            {
                var info = prop.Value;
                if (resolvedProperties.Contains(info))
                    continue;
                context.ResolvePropertyInjection(info, prop.Key);
            }

            //resolving injection properties, that doesn't registered in the "PropertyInjections"
            var propsToInjectValue = DestType.GetProperties()
                .Where(x => !resolvedProperties.Contains(x));

            foreach (var prop in propsToInjectValue)
            {
                var valueInjector = new ValueInjector();

                if (valueInjector.IsMemberSuitable(prop))
                {
                    var source = valueInjector.ResolveSourceValue(prop);
                    if (source.Success)
                    {
                        context.MapProperty(prop, source.Value);
                        resolvedProperties.Add(prop);
                        continue;
                    }                    
                }

                var inject=prop.GetCustomAttribute<ShoudlInjectAttribute>();
                if (inject != null)
                {
                    context.ResolvePropertyValueInjection(prop, string.Empty);
                    resolvedProperties.Add(prop);
                }

            }

            var propsToInject = DestType.GetProperties()
               .Where(x => x.CustomAttributes.Any(FilterInjectNamedInstance) && !resolvedProperties.Contains(x));
            foreach (var prop in propsToInject)
            {
                var injectType = prop.GetCustomAttribute<InjectInstanceAttribute>();
                context.ResolvePropertyNamedInstance(prop, injectType.Name);
            }
            var publicNotIndexedProperties = GetPublicNotIndexedProperties(DestType).Except(resolvedProperties);

            foreach (PropertyInfo publicNotIndexedProperty in publicNotIndexedProperties)
            {
                context.ResolvePublicNotIndexedProperty(publicNotIndexedProperty);
            }
            var customResolverValues = context.ResolvePropertiesCustom(resolvedProperties);
            foreach (var customResolverValue in customResolverValues)
            {
                customResolverValue.Key.SetValue(instance, customResolverValue.Value);
            }
        }

        public void CallInitMethod(TypeMaperContext context)
        {
            foreach (var method in InitMethods)
	        {
                var paramsDef = method.GetParameters();
                var paramValues = ResolvePramValues(paramsDef, method.Name, context.DestType.FullName, context);
                method.Invoke(context.Instance,paramValues.ToArray());
	        }
        }
    }


    public class TypeBuilderStub : TypeBuilder
    {
        public TypeBuilderStub(Type destType): base(destType)
        {
        }

        public override TypeBuilerContext CreateBuildingContext()
        {
            return new TypeBuilerContextStub(DestType, new Dictionary<PropertyInfo, ITypeMapper>());
        }
    }




    public abstract class TypeBuilerContext
    {

        private Dictionary<PropertyInfo, ITypeMapper> properyMappers;
        private List<ITypeMapper> typeMappers= new List<ITypeMapper>(); 
        public IList<PropertyInfo> ResolvedProperties { get; private set; }
        public object Instance { get; set; }
        public Type DestType { get; private set; }

        protected TypeBuilerContext(Type destType, Dictionary<PropertyInfo, ITypeMapper> properyMappers)
        {
            DestType = destType;
            this.properyMappers = properyMappers;
            ResolvedProperties = new List<PropertyInfo>();

            RegisterTypeMapper(new ConverTypeMapper());
        }

        protected void RegisterTypeMapper(ITypeMapper typeMapper)
        {
            typeMappers.Add(typeMapper);
        }

        public abstract MappingResult ResolvePropertyInjectionByResolver(PropertyInfo propInfo, string name);

        public abstract MappingResult ResolvePropertyInjection(PropertyInfo propInfo, string injectionName);
        public abstract MappingResult ResolvePropertyValueInjection(PropertyInfo propInfo, string name);
        public abstract MappingResult ResolvePropertyNamedInstance(PropertyInfo propInfo, string name);

        public abstract MappingResult ResolvePublicNotIndexedProperty(PropertyInfo propInfo);

        public abstract Dictionary<PropertyInfo, object> ResolvePropertiesCustom(IList<PropertyInfo> resolvedProperties);

        public abstract bool ResolveParameter(ParameterInfo paramInfo, string methodName, out object value);


        public MappingResult MapProperty(PropertyInfo propertyInfo, object value)
        {
            if(properyMappers.ContainsKey(propertyInfo))
            {
                var conversionResult=properyMappers[propertyInfo].Map(value,propertyInfo.PropertyType);
                if (conversionResult.Success)
                {
                    propertyInfo.SetValue(Instance, conversionResult.Value);
                    ResolvedProperties.Add(propertyInfo);
                    return MappingResult.Resolved;
                }

            }

            var valueMappers = typeMappers.ToArray();

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < valueMappers.Length; i++)
            {    
                var typeMapper = valueMappers[i];
                var mappingResult = typeMapper.Map(value,propertyInfo.PropertyType);
                if (mappingResult.Success)
                {
                    propertyInfo.SetValue(Instance, value);
                    ResolvedProperties.Add(propertyInfo);
                    return MappingResult.Resolved;
                }
            }

            return MappingResult.NotResolved;
        }
    }
    
    public class TypeBuilerContextStub:TypeBuilerContext
    {

        public override bool ResolveParameter(ParameterInfo paramInfo, string methodName, out object value)
        {
            value = null;
            return false;
        }

        public override MappingResult ResolvePropertyInjectionByResolver(PropertyInfo propInfo, string name)
        {
            return MappingResult.NotResolved;
        }

        public override MappingResult ResolvePropertyInjection(PropertyInfo propInfo, string injectionName)
        {
            return MappingResult.NotResolved;
        }

        public override MappingResult ResolvePropertyValueInjection(PropertyInfo propInfo, string name)
        {
            return MappingResult.NotResolved;
        }

        public override MappingResult ResolvePropertyNamedInstance(PropertyInfo propertyType, string name)
        {
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

        public TypeBuilerContextStub(Type destType, Dictionary<PropertyInfo, ITypeMapper> properyMappers) : base(destType,properyMappers)
        {
        }
    }

    public enum MappingResult
    {
        NotResolved=0,
        Resolved=1
    }
   

}