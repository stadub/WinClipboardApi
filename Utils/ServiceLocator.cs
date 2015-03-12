using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Utils.ServiceLocatorInfo;

namespace Utils
{
    [DebuggerDisplay(
        "Id = {Id} [Inst:{registeredInstances.Count} Init:{registeredInitializers.Count} T:{registeredTypes.Count}]"
        //+"{{Instances = {registeredInstances.Count} " //+
        //"Initializers = {registeredInitializers.Count} " +
        //"Types = {registeredTypes.Count}}}"
        , Name = "ServiceLocator{Id}")]
    public class ServiceLocator:IDisposable
    {
        public int Id { get; private set; }
        private static int _curId;
        public ServiceLocator()
        {
            Id = _curId;
            Interlocked.Increment(ref _curId);
        }
        protected Dictionary<KeyValuePair<Type, string>, object> registeredInstances = new Dictionary<KeyValuePair<Type, string>, object>();
        protected Dictionary<KeyValuePair<Type,string>, object> registeredInitializers = new Dictionary<KeyValuePair<Type, string>, object>();

        private Dictionary<KeyValuePair<Type, string>, TypeRegistrationInfo> registeredTypes = new Dictionary<KeyValuePair<Type, string>, TypeRegistrationInfo>();

        #region Register methods
        public LocatorRegistrationInfo<TClass> RegisterType<TInterface, TClass>() where TClass:TInterface
        {
            return RegisterType<TInterface, TClass>(string.Empty);
        }

        public LocatorRegistrationInfo<TClass> RegisterType<TInterface, TClass>(string name) where TClass : TInterface
        {
            var interfaceType = typeof(TInterface);
            var classType = typeof(TClass);

            if (interfaceType.IsGenericType || classType.IsGenericType)
                throw new TypeNotSupportedException(interfaceType,"Generic Types cannot be registered by RegisterType registrationInfo registration");
            
            var key = GetKey(interfaceType, name);
            CheckDuplicated(key);

            var registrationInfo = new TypeRegistrationInfo(classType, name);
            registeredTypes.Add(key,registrationInfo);
            return new LocatorRegistrationInfo<TClass>(registrationInfo);
        }

        public void RegisterInitializer<TInterface>(Func<TInterface> typeResolver)
        {
            RegisterInitializer(typeResolver, string.Empty);
        }

        public void RegisterInitializer<TInterface>(Func<TInterface> typeResolver, string name)
        {
            var interfaceType = typeof(TInterface);
            var key = GetKey(interfaceType, name);
            CheckDuplicated(key);
            registeredInitializers.Add(new KeyValuePair<Type, string>(interfaceType, name), typeResolver);
        }

        private void RegisterInstance<TInterface, TValue>(TValue value, string name) where TValue : class, TInterface
        {
            var interfaceType = typeof(TInterface);
            var key = GetKey(interfaceType, name);
            CheckDuplicated(key);

            registeredInstances[key] = value;
        }

        public void RegisterInstance<TInterface, TValue>(TValue value) where TValue : class,TInterface
        {
            RegisterInstance<TInterface, TValue>(value, string.Empty);
        }

        protected virtual void CheckDuplicated(KeyValuePair<Type, string> interfaceType)
        {
            if (registeredTypes.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType.Key, "Type-Type pair allready registered (via RegisterType)");

            if (registeredInstances.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType.Key, "Type instance allready registered (via RegisterInstance)");

            if (registeredInitializers.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType.Key, "Type-Type pair allready registered (via RegisterType)");
        }
        #endregion

        #region Resolvers
        public T Resolve<T>()
        {
            var @type = typeof(T);
            return (T)Resolve(@type, string.Empty);
        }

        protected object Resolve(Type @type,string name)
        {
            object result;
            if (TryResolveInstance(@type, name, out result))
                return result;
            if (TryResolveInitializer(@type, out result))
                return result;
            if (TryConstructType(@type,name,out result))
                return result;
            throw new TypeNotResolvedException(@type);
        }

        public T ResolveType<T>(string name)
        {
            object result;
            var @type = typeof(T);
            if (!TryConstructType(@type, name, out result))
                throw new TypeNotResolvedException(typeof(T));
            return (T)result;
        }

        public T ResolveType<T>()
        {
            return ResolveType<T>(string.Empty);
        }

        public T ResolveInstance<T>()
        {
            return ResolveInstance<T>(string.Empty);
        }

        private T ResolveInstance<T>(string name)
        {
            object result;
            var @type = typeof(T);
            if (!TryResolveInstance(@type,name,out result))
                throw new TypeNotResolvedException(typeof(T));

            return (T)result;
        }

        public T ResolveInitializer<T>()
        {
            object result;
            var @type = typeof(T);
            if (!TryResolveInitializer(@type, out result))
                throw new TypeNotResolvedException(typeof(T));

            return (T)result;
        }

        protected virtual bool TryResolveInstance(Type @type,string name,out object value)
        {
            var key = GetKey(type, name);
            if (registeredInstances.ContainsKey(key))
            {
                value = registeredInstances[key];
                return true;
            }
            value = TypeHelpers.GetDefault(@type);
            return false;
        }

        protected virtual bool TryResolveInitializer(Type @type, string name, out object value)
        {
            var initalizerKey = GetKey(@type, name);
            if (registeredInitializers.ContainsKey(initalizerKey))
            {
                Delegate Initializer = (Delegate)registeredInitializers[initalizerKey];
                value = Initializer.DynamicInvoke();
                return true;
            }
            value = null;
            return false;
        }

        private static KeyValuePair<Type, string> GetKey(Type @type, string name)
        {
            return new KeyValuePair<Type, string>(@type, name);
        }

        protected virtual bool TryResolveInitializer(Type @type, out object value)
        {
            return TryResolveInitializer(@type,string.Empty, out value);
        }
        #endregion

        protected virtual bool TryConstructType(Type type, string name, out object value)
        {
            var key = GetKey(type, name);
            if (!registeredTypes.ContainsKey(key))
            {
                value = TypeHelpers.GetDefault(@type);
                return false;
            }

            var registration = registeredTypes[key];
            if (registration == null) throw new TypeNotResolvedException(type,"Cannot find type registration");

            var ctor = registration.TryGetConstructor();

            //value types have no default constructor and shpuld be initalized by default value
            if (ctor == null && registration.DestType.IsValueType)
                value=TypeHelpers.GetDefault(registration.DestType);
            else
                value = CreateInstance(registration.DestType, type, ctor);

            InjectTypeProperties(value, registration);
            return true;
        }

        protected object CreateInstance(Type destType, Type sourceType, ConstructorInfo ctor)
        {
            //resolving constructor parametrs
            var paramsDef = ctor.GetParameters();

            if (paramsDef.Length == 0)
                return Activator.CreateInstance(destType);

            var paramValues = new List<object>();
            foreach (var paramDef in paramsDef)
            {
                var parametrType = paramDef.ParameterType;
                if (paramDef.IsOut || parametrType.IsByRef)
                {
                    throw new TypeNotSupportedException(sourceType, "Constructors with Out and Ref Attributes are not supported");
                }

                object paramValue = null;
                //optional parametr - defaut value or default(T)
                if (paramDef.IsOptional)
                {
                    paramValue = paramDef.HasDefaultValue ? paramDef.DefaultValue : TypeHelpers.GetDefault(parametrType);
                }
                //Inject parametr value from Inject Attribute
                else if (paramDef.CustomAttributes.Any(FilterInjectValue))
                {
                    var attribute = paramDef.GetCustomAttribute<InjectValueAttribute>();
                    if (attribute.Value != null)
                    {
                        var convertedType = Convert.ChangeType(attribute.Value, parametrType);
                        paramValue = convertedType;
                    }
                }
                //Trying to resolve registrationInfo from registered in the Locator
                else
                {
                    var name=string.Empty;
                    var attribute = paramDef.GetCustomAttribute<InjectInstanceAttribute>();
                    if (attribute != null) name = attribute.Name;
                    paramValue = Resolve(parametrType, name);
                }
                paramValues.Add(paramValue);
            }
            return ctor.Invoke(paramValues.ToArray());
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

        private void InjectTypeProperties(object instance, TypeRegistrationInfo type)
        {
            var resolvedProperties=new List<PropertyInfo>();
            //Inject registered properties values
            foreach (var prop in type.PropertyValueResolvers)
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
            foreach (var prop in type.PropertyInjections)
            {
                var propertyType = prop.Value;
                if (resolvedProperties.Contains(propertyType))
                    continue;
                var propValue = Resolve(propertyType.PropertyType, prop.Key);
                propertyType.SetValue(instance, propValue);
                resolvedProperties.Add(propertyType);
            }

            //resolving injection properties, that doesn't registered in the "PropertyInjections"
            var propsToInjectValue = type.DestType.GetProperties()
                .Where(x => x.CustomAttributes.Any(FilterInjectValue) && !resolvedProperties.Contains(x));

            foreach (var prop in propsToInjectValue)
            {
                var inject = prop.GetCustomAttribute<InjectValueAttribute>();
                if (inject.Value != null)
                {
                    if (!prop.PropertyType.IsInstanceOfType(inject.Value)) {
                        var convertedValue = Convert.ChangeType(inject.Value, prop.PropertyType);
                        prop.SetValue(instance, convertedValue);
                    }
                    else
                        prop.SetValue(instance, inject.Value);
                }
                else
                {
                    var propValue = Resolve(prop.PropertyType, string.Empty);
                    prop.SetValue(instance, propValue);
                }
                resolvedProperties.Add(prop);
            }

            var propsToInject = type.DestType.GetProperties()
               .Where(x => x.CustomAttributes.Any(FilterInjectNamedInstance) && !resolvedProperties.Contains(x));
            foreach (var prop in propsToInject)
            {
                var injectType = prop.GetCustomAttribute<InjectInstanceAttribute>();
                var propValue = Resolve(prop.PropertyType, injectType.Name);
                prop.SetValue(instance, propValue);
            }
        }

#region Dispose
        bool disposed=false;
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 

            if (disposing) {
                foreach (var instance in registeredInstances){
                    var disposable =instance.Value as IDisposable;
                    if(disposable!=null)
                        disposable.Dispose();
                }
            }

            disposed = true;
        }

        ~ServiceLocator()
        {
            Dispose(false);
        }
#endregion


    }

    [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    public sealed class UseConstructorAttribute : Attribute  {   }


    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class InjectValueAttribute : Attribute {
        public object Value{get;set;}
        public InjectValueAttribute()
        {
        }
        public InjectValueAttribute(string value)
        {
            Value=value;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class InjectInstanceAttribute : Attribute
    {
        public string Name{ get; set; }
        public InjectInstanceAttribute()
        {
            Name = string.Empty;
        }
        public InjectInstanceAttribute(string name)
        {
            Name = name;
        }
    }

    #region ServiceLocatorExceptions
    [Serializable]
    public class ServiceLocatorException : Exception
    {
        public Type Type{get;set;}
        public ServiceLocatorException(Type type) { Type=type;}
        public ServiceLocatorException(Type type, string message) : base(message) {Type=type; }
        public ServiceLocatorException(Type type, string message, Exception inner) : base(message, inner) { Type=type;}
        protected ServiceLocatorException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TypeNotResolvedException : ServiceLocatorException
    {
        public TypeNotResolvedException(Type type):base(type) {}
        public TypeNotResolvedException(Type type, string message) : base(type,message) {}
        public TypeNotResolvedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected TypeNotResolvedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
    
    [Serializable]
    public class TypeNotSupportedException : ServiceLocatorException
    {
        public TypeNotSupportedException(Type type):base(type) {}
        public TypeNotSupportedException(Type type, string message) : base(type,message) {}
        public TypeNotSupportedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected TypeNotSupportedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ConstructorNotResolvedException : ServiceLocatorException
    {
        public ConstructorNotResolvedException(Type type):base(type) {}
        public ConstructorNotResolvedException(Type type, string message) : base(type,message) {}
        public ConstructorNotResolvedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected ConstructorNotResolvedException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TypeAllreadyRegisteredException : ServiceLocatorException
    {
        public TypeAllreadyRegisteredException(Type type) : base(type) { }
        public TypeAllreadyRegisteredException(Type type, string message) : base(type, message) { }
        public TypeAllreadyRegisteredException(Type type, string message, Exception inner) : base(type, message, inner) { }
        protected TypeAllreadyRegisteredException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
#endregion


   
    

}
