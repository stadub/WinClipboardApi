using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Utils
{
    public partial class ServiceLocator:IDisposable
    {
        Dictionary<Type, object> registeredInstances = new Dictionary<Type, object>();
        Dictionary<Type, object> registeredInitializers = new Dictionary<Type, object>();

        Dictionary<Type, List<TypeRegistrationInfo>> registeredTypes = new Dictionary<Type, List<TypeRegistrationInfo>>();

        #region Register methods
        public LocatorRegistrationInfo<TClass> RegisterType<TInterface, TClass>()
        {
            return RegisterType<TInterface, TClass>(string.Empty);
        }

        public LocatorRegistrationInfo<TClass> RegisterType<TInterface, TClass>(string name)
        {
            var interfaceType = typeof(TInterface);
            var classType = typeof(TClass);

            if (interfaceType.IsGenericType || classType.IsGenericType)
                throw new TypeNotSupportedException(interfaceType,"Generic Types cannot be registered by RegisterType registrationInfo registration");

            CheckDuplicated(interfaceType, name);

            if (!registeredTypes.ContainsKey(interfaceType))
                registeredTypes[interfaceType]=new List<TypeRegistrationInfo>();

            var registrationInfos = registeredTypes[interfaceType];
            var registrationInfo = new TypeRegistrationInfo(classType, name);
            registrationInfos.Add(registrationInfo);
            return new LocatorRegistrationInfo<TClass>(registrationInfo);
        }

        public void RegisterInitializer<TInterface>(Func<TInterface> typeResolver)
        {
            var interfaceType = typeof(TInterface);
            CheckDuplicated(interfaceType,string.Empty);
            registeredInitializers.Add(interfaceType, typeResolver);
        }

        public void RegisterInstance<TInterface, TValue>(TValue value) where TValue : class
        {
            var interfaceType = typeof(TInterface);
            CheckDuplicated(interfaceType,string.Empty);
            registeredInstances.Add(interfaceType, value);
        }


        protected virtual void CheckDuplicated(Type interfaceType,string name)
        {
            if (registeredTypes.ContainsKey(interfaceType))
            {
                var allInstances=registeredTypes[interfaceType];
                var index = allInstances.FindIndex(info => info.RegistrationName == name);
                if (index!=-1)
                    throw new TypeAllreadyRegisteredException(interfaceType, "Type-Type pair allready registered (via RegisterType)");

            }
                
            if (registeredInstances.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType, "Type instance allready registered (via RegisterInstance)");

            if (registeredInitializers.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType, "Type allready registered (via RegisterInitializer)");
        }
        #endregion
        #region Resolvers



        public T Resolve<T>()
        {
            var @type = typeof(T);
            return (T)Resolve(@type);
        }

        public object Resolve(Type @type)
        {
            object result;
            if (TryResolveInstance(@type,out result))
                return result;
            if (TryResolveInitializer(@type, out result))
                return result;
            if (TryConstructType(@type,string.Empty,out result))
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
            object result;
            var @type = typeof(T);
            if (!TryResolveInstance(@type,out result))
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

        protected virtual bool TryResolveInstance(Type @type,out object value)
        {
            if (registeredInstances.ContainsKey(@type))
            {
                value = registeredInstances[@type];
                return true;
            }
            value = TypeHelpers.GetDefault(@type);
            return false;
        }

        protected virtual bool TryResolveInitializer(Type @type, out object value)
        {
            if (registeredInitializers.ContainsKey(@type))
            {
                Delegate Initializer = (Delegate)registeredInitializers[@type];
                value=Initializer.DynamicInvoke();
                return true;
            }
            value = null;
            return false;
        }
        #endregion

        protected virtual bool TryConstructType(Type type, string name, out object value)
        {
            if (!registeredTypes.ContainsKey(@type))
            {
                value = TypeHelpers.GetDefault(@type);
                return false;
            }

            var typeRegistrations= registeredTypes[@type];
            var registration=typeRegistrations.FirstOrDefault(info => info.RegistrationName == name);
            if (registration == null) throw new TypeNotResolvedException(type,"Cannot find type registration");
            var ctor = GetConstructor(registration);

            value = CreateInstance(registration.DestType,type, ctor);

            InjectTypeProperties(value, registration);
            return true;
        }

        private object CreateInstance(Type destType, Type sourceType, ConstructorInfo ctor)
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
                else if (paramDef.CustomAttributes.Any(FilterInjectParametrs))
                {
                    var attribute = paramDef.GetCustomAttribute<InjectAttribute>();
                    if (attribute.Value != null)
                    {
                        var convertedType = Convert.ChangeType(attribute.Value, parametrType);
                        paramValue = convertedType;
                    }
                }
                //Typing to resolve registrationInfo from registered in the Locator
                else
                {
                    paramValue = Resolve(parametrType);
                }
                paramValues.Add(paramValue);
            }
            return ctor.Invoke(paramValues.ToArray());
        }


        private ConstructorInfo GetConstructor(TypeRegistrationInfo type)
        {
            //enumerating only public ctors
            var ctors = type.DestType.GetConstructors();

            //search for constructor marked as [UseConstructor]
            foreach (var ctor in ctors)
            {
                var attributes=ctor.GetCustomAttributes(typeof(UseConstructorAttribute),false);
                if(attributes.Any())
                    return ctor;
            }
            //try to find default constructor
            foreach (var ctor in ctors)
            {
                var args=ctor.GetParameters();
                if(args.Length==0)
                    return ctor;
            }
            //try to use first public
            if(ctors.Length>0)
                return ctors[0];
            throw new ConstructorNotResolvedException(type.DestType);
        }

        static Type injectionFlagType = typeof(InjectAttribute);
        static Func<CustomAttributeData, bool> injectField = (x) => x.AttributeType.IsAssignableFrom(injectionFlagType);
        protected static bool FilterInjectParametrs(CustomAttributeData field)
        {
            //declare property/ctor parametr filter signature
            return injectField(field);
        }

        protected void InjectTypeProperties(object instance, TypeRegistrationInfo type)
        {
            //Inject registered property injections
            var injectProperties = type.PropertyInjections;
            foreach (var prop in injectProperties)
            {
                var propValue = Resolve(prop.PropertyType);
                prop.SetValue(instance, propValue);
            }

            //resolving injection properties, that doesn't registered in the "PropertyInjections"
            var propsToInject = type.DestType.GetProperties()
                .Where(x => x.CustomAttributes.Any(FilterInjectParametrs) && !injectProperties.Contains(x));

            foreach(var prop in propsToInject){
                var propValue=Resolve(prop.PropertyType);
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
    public sealed class InjectAttribute : Attribute {
        public object Value{get;set;}
        public InjectAttribute()
        {
        }
        public InjectAttribute(string value)
        {
            Value=value;
        }
    }



    public class TypeRegistrationInfo
    {
        public TypeRegistrationInfo(Type destType, string registrationName)
        {
            PropertyInjections = new List<PropertyInfo>();
            DestType = destType;
            RegistrationName = registrationName;
        }
        public List<PropertyInfo> PropertyInjections { get; private set; }
        public Type DestType { get; private set; }
        public string RegistrationName { get; private set; }
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
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TypeNotResolvedException : ServiceLocatorException
    {
        public TypeNotResolvedException(Type type):base(type) {}
        public TypeNotResolvedException(Type type, string message) : base(type,message) {}
        public TypeNotResolvedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected TypeNotResolvedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    
    [Serializable]
    public class TypeNotSupportedException : ServiceLocatorException
    {
        public TypeNotSupportedException(Type type):base(type) {}
        public TypeNotSupportedException(Type type, string message) : base(type,message) {}
        public TypeNotSupportedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected TypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ConstructorNotResolvedException : ServiceLocatorException
    {
        public ConstructorNotResolvedException(Type type):base(type) {}
        public ConstructorNotResolvedException(Type type, string message) : base(type,message) {}
        public ConstructorNotResolvedException(Type type, string message, Exception inner) : base(type,message, inner) {}
        protected ConstructorNotResolvedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class TypeAllreadyRegisteredException : ServiceLocatorException
    {
        public TypeAllreadyRegisteredException(Type type) : base(type) { }
        public TypeAllreadyRegisteredException(Type type, string message) : base(type, message) { }
        public TypeAllreadyRegisteredException(Type type, string message, Exception inner) : base(type, message, inner) { }
        protected TypeAllreadyRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
#endregion


   
    

}
