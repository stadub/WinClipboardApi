using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        //may be in future type key should changed from <Type, Name> to <TypeName, Name> to reduce memory usage
        protected Dictionary<KeyValuePair<String, string>, object> registeredInstances = new Dictionary<KeyValuePair<String, string>, object>();
        protected Dictionary<KeyValuePair<String, string>, object> registeredInitializers = new Dictionary<KeyValuePair<String, string>, object>();
        private Dictionary<KeyValuePair<String, string>, TypeBuilder> registeredTypes = new Dictionary<KeyValuePair<String, string>, TypeBuilder>();

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
                throw new TypeNotSupportedException(interfaceType.FullName,"Generic Types cannot be registered by RegisterType registrationInfo registration");
            
            var key = GetKey(interfaceType, name);
            CheckDuplicated(key);

            var registrationInfo = new LocatorTypeBuilder(this,classType);
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
            registeredInitializers.Add(new KeyValuePair<String, string>(interfaceType.FullName, name), typeResolver);
        }

        public void RegisterInstance<TInterface, TValue>(TValue value, string name) where TValue : class, TInterface
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

        protected virtual void CheckDuplicated(KeyValuePair<String, string> interfaceType)
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
            if (TryResolve(@type, name, out result))
                return result;
            throw new TypeNotResolvedException(type.FullName);
        }


        protected internal bool TryResolve(Type @type, string name, out object result)
        {
            if (TryResolveInstance(@type, name, out result))
                return true;
            if (TryResolveInitializer(@type, out result))
                return true;
            if (TryConstructType(@type, name, out result))
                return true;
            return false;
        }

        public T ResolveType<T>(string name)
        {
            object result;
            var @type = typeof(T);
            if (!TryConstructType(@type, name, out result))
                throw new TypeNotResolvedException(type.FullName);
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
                throw new TypeNotResolvedException(type.FullName);

            return (T)result;
        }

        public T ResolveInitializer<T>()
        {
            object result;
            var @type = typeof(T);
            if (!TryResolveInitializer(@type, out result))
                throw new TypeNotResolvedException(type.FullName);

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

        private static KeyValuePair<String, string> GetKey(Type @type, string name)
        {
            return new KeyValuePair<String, string>(@type.FullName, name);
        }

        protected virtual bool TryResolveInitializer(Type @type, out object value)
        {
            return TryResolveInitializer(@type,string.Empty, out value);
        }
        #endregion

        protected virtual bool TryConstructType(Type type, string name, out object value)
        {
            var key = GetKey(type, name);
            TypeBuilder registration;
            if (!registeredTypes.ContainsKey(key))
            {
#if RESOLVE_NON_REGISTERED
                registration= new LocatorTypeBuilder(this,type,name);
#else
                value = TypeHelpers.GetDefault(@type);
                return false;
#endif
            }
            else
            {
                registration = registeredTypes[key];
            }
            
            if (registration == null) throw new TypeNotResolvedException(type.FullName,"Cannot find type registration");

            var ctor = registration.TryGetConstructor();

            //value types have no default constructor and shpuld be initalized by default value
            if (ctor == null && registration.DestType.IsValueType)
                value=TypeHelpers.GetDefault(registration.DestType);
            else
                value = registration.CreateInstance(ctor,type);

            registration.InjectTypeProperties(value);
            return true;
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
        public String TypeName{get;set;}
        public ServiceLocatorException(String typeName) { TypeName = typeName; }
        public ServiceLocatorException(String typeName, string message) : base(message) { TypeName = typeName; }
        public ServiceLocatorException(String typeName, string message, Exception inner) : base(message, inner) { TypeName = typeName; }
        protected ServiceLocatorException(SerializationInfo info,StreamingContext context): base(info, context) { }
    }

    [Serializable]
    public class TypeNotResolvedException : ServiceLocatorException
    {
        public TypeNotResolvedException(String typeName) : base(typeName) { }
        public TypeNotResolvedException(String typeName, string message) : base(typeName, message) { }
        public TypeNotResolvedException(String typeName, string message, Exception inner) : base(typeName, message, inner) { }
        protected TypeNotResolvedException(SerializationInfo info,StreamingContext context): base(info, context) { }
    }
    
    [Serializable]
    public class TypeNotSupportedException : ServiceLocatorException
    {
        public TypeNotSupportedException(String typeName) : base(typeName) { }
        public TypeNotSupportedException(String typeName, string message) : base(typeName, message) { }
        public TypeNotSupportedException(String typeName, string message, Exception inner) : base(typeName, message, inner) { }
        protected TypeNotSupportedException(SerializationInfo info,StreamingContext context): base(info, context) { }
    }
    
    [Serializable]
    public class TypeInitalationException : TypeNotResolvedException
    {
        public TypeInitalationException(String typeName) : base(typeName) { }
        public TypeInitalationException(String typeName, string message) : base(typeName, message) { }
        public TypeInitalationException(String typeName, string message, Exception inner) : base(typeName, message, inner) { }
        protected TypeInitalationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class ConstructorNotResolvedException : ServiceLocatorException
    {
        public ConstructorNotResolvedException(String typeName) : base(typeName) { }
        public ConstructorNotResolvedException(String typeName, string message) : base(typeName, message) { }
        public ConstructorNotResolvedException(String typeName, string message, Exception inner) : base(typeName, message, inner) { }
        protected ConstructorNotResolvedException(SerializationInfo info,StreamingContext context): base(info, context) { }
    }

    [Serializable]
    public class TypeAllreadyRegisteredException : ServiceLocatorException
    {
        public TypeAllreadyRegisteredException(String typeName) : base(typeName) { }
        public TypeAllreadyRegisteredException(String typeName, string message) : base(typeName, message) { }
        public TypeAllreadyRegisteredException(String typeName, string message, Exception inner) : base(typeName, message, inner) { }
        protected TypeAllreadyRegisteredException(SerializationInfo info,StreamingContext context): base(info, context) { }
    }

    [Serializable]
    public class PropertyMappingException : TypeInitalationException
    {
        public string Property { get; private set; }

        public PropertyMappingException(String typeName,String property) : base(typeName) { Property = property; }

        public PropertyMappingException(String typeName, String property, string message) : base(typeName, message) { Property = property; }
        public PropertyMappingException(String typeName, String property, string message, Exception inner) : base(typeName, message, inner) { Property = property; }
        protected PropertyMappingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
#endregion
}
