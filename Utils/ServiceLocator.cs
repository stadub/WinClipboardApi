using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Utils
{
    public class ServiceLocator:IDisposable
    {
        Dictionary<Type, Type> registeredTypes = new Dictionary<Type, Type>();
        Dictionary<Type, object> registeredInstances = new Dictionary<Type, object>();
        Dictionary<Type, object> registeredInitializers = new Dictionary<Type, object>();

        #region Register methods
        public void RegisterType<TInterface, TClass>()
        {
            var interfaceType = typeof(TInterface);
            var classType = typeof(TClass);

            if (interfaceType.IsGenericType || classType.IsGenericType)
                throw new TypeNotSupportedException(interfaceType,"Generic Types cannot be registered by RegisterType type registration");

            CheckDuplicated(interfaceType);
            registeredTypes.Add(interfaceType, classType);
        }


        public void RegisterInitializer<TInterface>(Func<TInterface> typeResolver)
        {
            var interfaceType = typeof(TInterface);
            CheckDuplicated(interfaceType);
            registeredInitializers.Add(interfaceType, typeResolver);
        }

        public void RegisterInstance<TInterface, TValue>(TValue value) where TValue : class
        {
            var interfaceType = typeof(TInterface);
            CheckDuplicated(interfaceType);
            registeredInstances.Add(interfaceType, value);
        }

        protected virtual void CheckDuplicatedGeneric(Type genericType,Type type)
        {
            
        }
        protected virtual void CheckDuplicated(Type interfaceType)
        {
            if (registeredTypes.ContainsKey(interfaceType))
                throw new TypeAllreadyRegisteredException(interfaceType, "Type-Type pair allready registered (via RegisterType)");
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
            if (TryConstructType(@type,out result))
                return result;
            throw new TypeNotResolvedException(@type);
        }


        public T ResolveType<T>()
        {
            object result;
            var @type = typeof(T);
            if (!TryConstructType(@type, out result))
                throw new TypeNotResolvedException(typeof(T));
            return (T)result;
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

        protected virtual bool TryConstructType(Type @type,out object value)
        {
            if (!registeredTypes.ContainsKey(@type))
            {
                value = TypeHelpers.GetDefault(@type);
                return false;
            }

            var destType = registeredTypes[@type];

            var ctor = GetConstructor(destType);

            //resolving constructor parametrs
            var paramsDef=ctor.GetParameters();

            if(paramsDef.Length==0)
                value = Activator.CreateInstance(destType);
            else
            {
                var paramValues= new List<object>();
                foreach(var paramDef in paramsDef)
                {
                    var parametrType = paramDef.ParameterType;
                    if (paramDef.IsOut || parametrType.IsByRef)
                        throw new TypeNotSupportedException(@type,"Constructors with Out and Ref Attributes are not supported");
                    
                    object paramValue = null;
                    //optional parametr - defaut value or default(T)
                    if (paramDef.IsOptional)
                    {
                        paramValue = paramDef.HasDefaultValue ? paramDef.DefaultValue : TypeHelpers.GetDefault(parametrType);
                    }
                    //Inject parametr value from Inject Attribute
                    else if (paramDef.CustomAttributes.Any(FilterInjectProperties))
                    {
                        var attribute = paramDef.GetCustomAttribute<InjectAttribute>();
                        if(attribute.Value!=null){
                            var convertedType = Convert.ChangeType(attribute.Value, parametrType);
                            paramValue=convertedType;
                        }
                    }
                    //Typing to resolve type from registered in the Locator
                    else
                    {
                        paramValue = Resolve(parametrType);
                    }
                    paramValues.Add(paramValue);
                }
                value = ctor.Invoke(paramValues.ToArray());
            }

            //resolving injection properties
            var propsToInject = destType.GetProperties()
                .Where(x => x.CustomAttributes.Any(FilterInjectProperties));

            foreach(var prop in propsToInject){
                var propValue=Resolve(prop.PropertyType);
                prop.SetValue(value,propValue);
            }
            return true;
        }

        
        private ConstructorInfo GetConstructor(Type type)
        {
            //enumerating only public ctors
            var ctors = type.GetConstructors();

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
            throw new ConstructorNotResolvedException(type);
        }

        static Type injectionFlagType = typeof(InjectAttribute);
        static Func<CustomAttributeData, bool> injectField = (x) => x.AttributeType.IsAssignableFrom(injectionFlagType);
        protected static bool FilterInjectProperties(CustomAttributeData field)
        {
            //declare property/ctor parametr filter signature
            return injectField(field);
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
