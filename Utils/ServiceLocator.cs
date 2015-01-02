using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Utils
{
    public class ServiceLocator
    {
        Dictionary<Type, Type> registeredTypes = new Dictionary<Type, Type>();
        Dictionary<Type, object> registeredInstances = new Dictionary<Type, object>();

        public void Register<TInterface, TClass>()
        {
            registeredTypes.Add(typeof(TInterface), typeof(TClass));
        }

        public void Register<TInterface, TValue>(TValue value)
        {
            registeredInstances.Add(typeof(TInterface), value);
        }

        public T Resolve<T>()
        {
            var @type = typeof(T);
            return (T)Resolve(@type);
        }

        protected object Resolve(Type @type)
        {
            object result;
            if (TryResolveInstance(@type,out result))
                return result;
            if (TryConstructType(@type,out result))
                return result;
            throw new TypeNotResolvedException(@type);
        }

        public T ResolveInstance<T>()
        {
            object result;
            var @type = typeof(T);
            if (!TryResolveInstance(@type,out result))
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
                value=Activator.CreateInstance(@type);
            else
            {
                var paramValues= new List<object>();
                foreach(var paramDef in paramsDef){
                    if (paramDef.IsOptional && !paramDef.CustomAttributes.Any(FilterInjectProperties))
                        continue;
                    paramValues.Add(Resolve(paramDef.ParameterType));
                }
                value = ctor.Invoke(paramValues.ToArray());
            }

            //resolving injection properties
            var propsToInject= @type.GetProperties()
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
    }

    [AttributeUsage(AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    sealed class UseConstructorAttribute : Attribute  {   }


    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    sealed class InjectAttribute : Attribute { }


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
}
