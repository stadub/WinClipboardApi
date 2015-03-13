using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils
{
    public abstract class TypeBuilder
    {
        protected TypeBuilder(Type destType, string registrationName)
        {
            PropertyInjectionResolvers = new List<KeyValuePair<PropertyInfo, Type>>();
            PropertyValueResolvers = new List<KeyValuePair<PropertyInfo, object>>();
            PropertyInjections = new List<KeyValuePair<string, PropertyInfo>>();
            DestType = destType;
            RegistrationName = registrationName;
        }

        public List<KeyValuePair<string, PropertyInfo>> PropertyInjections { get; private set; }
        public Type DestType { get; private set; }
        public string RegistrationName { get; private set; }
        public List<KeyValuePair<PropertyInfo,Type>> PropertyInjectionResolvers { get; set; }
        public List<KeyValuePair<PropertyInfo,object>> PropertyValueResolvers { get; set; }


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

        public void InjectTypeProperties(object instance)
        {
            var resolvedProperties = new List<PropertyInfo>();
            //Inject registered properties values
            foreach (var prop in PropertyValueResolvers)
            {
                prop.Key.SetValue(instance, prop.Value);
                resolvedProperties.Add(prop.Key);
            }

#if PropertyInjectionResolvers
            //Inject registered property injectction resolvers
            foreach (var prop in type.PropertyInjectionResolvers)
            {
                if(resolvedProperties.Contains(prop.Key))
                    continue;
                var propValue = Resolve(prop.Value, string.Empty);
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
                var propValue = ResolvePropertyInjection(propertyType.PropertyType, prop.Key);
                propertyType.SetValue(instance, propValue);
                resolvedProperties.Add(propertyType);
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
                }
                else
                {
                    var propValue = ResolvePropertyValueInjection(prop.PropertyType, string.Empty);
                    prop.SetValue(instance, propValue);
                }
                resolvedProperties.Add(prop);
            }

            var propsToInject = DestType.GetProperties()
               .Where(x => x.CustomAttributes.Any(FilterInjectNamedInstance) && !resolvedProperties.Contains(x));
            foreach (var prop in propsToInject)
            {
                var injectType = prop.GetCustomAttribute<InjectInstanceAttribute>();
                var propValue = ResolvePropertyNamedInstance(prop.PropertyType, injectType.Name);
                prop.SetValue(instance, propValue);
            }
        }

        protected abstract object ResolvePropertyInjection(Type propertyType, string value);
        protected abstract object ResolvePropertyValueInjection(Type propertyType, string value);
        protected abstract object ResolvePropertyNamedInstance(Type propertyType, string value);
    }
}