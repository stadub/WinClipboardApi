using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
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
                    paramValueInjected = InjectParameterValue(paramDef, out paramValue);
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

        protected virtual bool InjectParameterValue(ParameterInfo paramInfo, out object value)
        {
            value = null;
            if (!paramInfo.CustomAttributes.Any(FilterInjectValue))
                return false;

            var parametrType = paramInfo.ParameterType;
            var attribute = paramInfo.GetCustomAttribute<InjectValueAttribute>();
            if (attribute.Value != null)
            {
                var convertedType = Convert.ChangeType(attribute.Value, parametrType);
                value = convertedType;
                return true;
            }
            return false;
        }



        protected static bool FilterInjectValue(CustomAttributeData field)
        {
            //declare property/ctor parametr filter signature
            return field.AttributeType.IsAssignableFrom(typeof(InjectValueAttribute));
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
                var result = ResolvePropertyInjectionByResolver(prop.Value, string.Empty, out propValue);
                if(result)
                    prop.Key.SetValue(instance, propValue);
                resolvedProperties.Add(prop.Key);
            }
#endif
            //Inject registered property injections
            foreach (var prop in PropertyInjections)
            {
                var info = prop.Value;
                if (resolvedProperties.Contains(info))
                    continue;
                Object propValue;
                if (context.ResolvePropertyInjection(info.Name, info.PropertyType, prop.Key, out propValue))
                {
                    info.SetValue(instance, propValue);
                    resolvedProperties.Add(info);
                }
            }

            //resolving injection properties, that doesn't registered in the "PropertyInjections"
            var propsToInjectValue = DestType.GetProperties()
                .Where(x => x.CustomAttributes.Any(FilterInjectValue) && !resolvedProperties.Contains(x));

            foreach (var prop in propsToInjectValue)
            {
                var inject = prop.GetCustomAttribute<InjectValueAttribute>();
                if (inject.Value != null)
                {
                    if (!prop.PropertyType.IsInstanceOfType(inject.Value))
                    {
                        var convertedValue = Convert.ChangeType(inject.Value, prop.PropertyType);
                        prop.SetValue(instance, convertedValue);
                    }
                    else
                        prop.SetValue(instance, inject.Value);
                    resolvedProperties.Add(prop);
                }
                else
                {
                    Object propValue;
                    if (context.ResolvePropertyValueInjection(prop.PropertyType, string.Empty, out propValue))
                    {
                        prop.SetValue(instance, propValue);
                        resolvedProperties.Add(prop);
                    }
                }
                
            }

            var propsToInject = DestType.GetProperties()
               .Where(x => x.CustomAttributes.Any(FilterInjectNamedInstance) && !resolvedProperties.Contains(x));
            foreach (var prop in propsToInject)
            {
                var injectType = prop.GetCustomAttribute<InjectInstanceAttribute>();
                Object propValue;
                if (context.ResolvePropertyNamedInstance(prop.PropertyType, injectType.Name, out propValue))
                {
                    prop.SetValue(instance, propValue);
                    resolvedProperties.Add(prop);
                }
            }
            var publicNotIndexedProperties = GetPublicNotIndexedProperties(DestType).Except(resolvedProperties);

            foreach (PropertyInfo publicNotIndexedProperty in publicNotIndexedProperties)
            {
                object value;
                if (context.ResolvePublicNotIndexedProperty(publicNotIndexedProperty, out value))
                {
                    publicNotIndexedProperty.SetValue(instance, value);
                    resolvedProperties.Add(publicNotIndexedProperty);
                }
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
            return new TypeBuilerContextStub(DestType);
        }
    }




    public abstract class TypeBuilerContext
    {
        public IList<PropertyInfo> ResolvedProperties { get; private set; }
        public object Instance { get; set; }
        public Type DestType { get; private set; }

        protected TypeBuilerContext(Type destType)
        {
            DestType = destType;
            ResolvedProperties = new List<PropertyInfo>();
        }

        public abstract bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value);

        public abstract bool ResolvePropertyInjection(string propertyName, Type propertyType, string injectionName, out object value);
        public abstract bool ResolvePropertyValueInjection(Type propertyType, string name, out object value);
        public abstract bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value);

        public abstract bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value);

        public abstract Dictionary<PropertyInfo, object> ResolvePropertiesCustom(IList<PropertyInfo> resolvedProperties);

        public abstract bool ResolveParameter(ParameterInfo paramInfo,string methodName, out object value);
    }
    
    public class TypeBuilerContextStub:TypeBuilerContext
    {
        
        public override bool ResolveParameter(ParameterInfo paramInfo,string methodName, out object value)
        {
            return NotResolved(out value);
        }

        public override bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        public override bool ResolvePropertyInjection(string propertyName, Type propertyType, string injectionName, out object value)
        {
            return NotResolved(out value);
        }

        public override bool ResolvePropertyValueInjection(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        public override bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        public override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value)
        {
            return NotResolved(out value);
        }

        public override Dictionary<PropertyInfo, object> ResolvePropertiesCustom(IList<PropertyInfo> resolvedProperties)
        {
            return new Dictionary<PropertyInfo, object>();
        }
        
        private static bool NotResolved(out object value)
        {
            value = null;
            return false;
        }

        public TypeBuilerContextStub(Type destType) : base(destType)
        {
        }
    }

    public sealed class PropertyMappingResilt
    {
        public static PropertyMappingResilt MapProperty()
        {

        }
        private PropertyMappingResilt()
        {

        }
        public static PropertyMappingResilt Resolved(){

        }
        public static PropertyMappingResilt NotResolved()
        {

        }
    }

}