using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public abstract class TypeBuilder 
    {
        protected TypeBuilder(Type destType)
        {
            PropertyInjectionResolvers = new List<KeyValuePair<PropertyInfo, Type>>();
            PropertyValueResolvers = new List<KeyValuePair<PropertyInfo, object>>();
            PropertyInjections = new List<KeyValuePair<string, PropertyInfo>>();
            DestType = destType;
            IgnoreProperties= new List<PropertyInfo>();
        }

        public List<KeyValuePair<string, PropertyInfo>> PropertyInjections { get; private set; }
        public Type DestType { get; private set; }

        public List<KeyValuePair<PropertyInfo,Type>> PropertyInjectionResolvers { get; private set; }
        public List<KeyValuePair<PropertyInfo,object>> PropertyValueResolvers { get; private set; }
        public List<PropertyInfo> IgnoreProperties { get; private set; }


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
            throw new ConstructorNotResolvedException(DestType);
        }

        public object CreateInstance(ConstructorInfo ctor, Type sourceType)
        {
            //resolving constructor parametrs
            var paramsDef = ctor.GetParameters();

            if (paramsDef.Length == 0)
                return Activator.CreateInstance(DestType);

            var paramValues = new List<object>();
            foreach (ParameterInfo paramDef in paramsDef)
            {
                var parametrType = paramDef.ParameterType;
                if (paramDef.IsOut || parametrType.IsByRef)
                {
                    throw new TypeNotSupportedException(sourceType, "Constructors with Out and Ref Attributes are not supported");
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
                    paramValueInjected = ResolveParameter(paramDef, out paramValue);
                }
                if (!paramValueInjected)
                    throw new TypeInitalationException(DestType, string.Format("Property {0} doesn't resolved", paramDef.Name));
                paramValues.Add(paramValue);
            }
            return ctor.Invoke(paramValues.ToArray());
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

        protected abstract bool ResolveParameter(ParameterInfo paramInfo, out object value);

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
        public void InjectTypeProperties(object instance)
        {
            var resolvedProperties = new List<PropertyInfo>();
            InjectTypeProperties(instance,resolvedProperties);
        }

        internal virtual void InjectTypeProperties(object instance,List<PropertyInfo> resolvedProperties)
        {
            IgnoreProperties.ForEach(resolvedProperties.Add);
            //Inject registered properties values
            foreach (var prop in PropertyValueResolvers)
            {
                prop.Key.SetValue(instance, prop.Value);
                resolvedProperties.Add(prop.Key);
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
                var propertyType = prop.Value;
                if (resolvedProperties.Contains(propertyType))
                    continue;
                Object propValue;
                if (ResolvePropertyInjection(propertyType.PropertyType, prop.Key, out propValue))
                {
                    propertyType.SetValue(instance, propValue);
                    resolvedProperties.Add(propertyType);
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
                    if (ResolvePropertyValueInjection(prop.PropertyType, string.Empty, out propValue))
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
                if (ResolvePropertyNamedInstance(prop.PropertyType, injectType.Name, out propValue))
                {
                    prop.SetValue(instance, propValue);
                    resolvedProperties.Add(prop);
                }
            }
            var publicNotIndexedProperties = GetPublicNotIndexedProperties(DestType).Except(resolvedProperties);

            foreach (PropertyInfo publicNotIndexedProperty in publicNotIndexedProperties)
            {
                object value;
                if (ResolvePublicNotIndexedProperty(publicNotIndexedProperty, out value))
                {
                    publicNotIndexedProperty.SetValue(instance, value);
                    resolvedProperties.Add(publicNotIndexedProperty);
                }
            }
            var customResolverValues = ResolvePropertiesCustom(resolvedProperties);
            foreach (var customResolverValue in customResolverValues)
            {
                customResolverValue.Key.SetValue(instance, customResolverValue.Value);
            }
        }

        protected abstract bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value);

        protected abstract bool ResolvePropertyInjection(Type propertyType, string name, out object value);
        protected abstract bool ResolvePropertyValueInjection(Type propertyType, string name, out object value);
        protected abstract bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value);

        protected abstract bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value);

        protected abstract Dictionary<PropertyInfo, object> ResolvePropertiesCustom(List<PropertyInfo> resolvedProperties);


    }

    public class TypeBuilderStub : TypeBuilder
    {
        public TypeBuilderStub(Type destType) : base(destType)
        {
        }

        private static bool NotResolved(out object value)
        {
            value = null;
            return false;
        }

        protected override bool ResolveParameter(ParameterInfo paramInfo, out object value)
        {
            return NotResolved(out value);
        }

        protected override bool ResolvePropertyInjectionByResolver(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        protected override bool ResolvePropertyInjection(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        protected override bool ResolvePropertyValueInjection(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        protected override bool ResolvePropertyNamedInstance(Type propertyType, string name, out object value)
        {
            return NotResolved(out value);
        }

        protected override bool ResolvePublicNotIndexedProperty(PropertyInfo propertyType, out object value)
        {
            return NotResolved(out value);
        }

        protected override Dictionary<PropertyInfo, object> ResolvePropertiesCustom(List<PropertyInfo> resolvedProperties)
        {
            return new Dictionary<PropertyInfo, object>();
        }
    }

    public class TypeBuilderProxy : TypeBuilderStub
    {
        private readonly TypeBuilder baseBuilder;


        public TypeBuilderProxy(Type destType, TypeBuilder baseBuilder) : base(destType)
        {
            this.baseBuilder = baseBuilder;
        }

        internal override void InjectTypeProperties(object instance, List<PropertyInfo> resolvedProperties)
        {
            base.InjectTypeProperties(instance, resolvedProperties);
            baseBuilder.InjectTypeProperties(instance, resolvedProperties);
        }

    }

}