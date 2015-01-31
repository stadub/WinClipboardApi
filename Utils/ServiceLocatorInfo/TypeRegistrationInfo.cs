using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.ServiceLocatorInfo
{
    public class TypeRegistrationInfo
    {
        public TypeRegistrationInfo(Type destType, string registrationName)
        {
            PropertyInjections = new List<PropertyInfo>();
            PropertyInjectionResolvers = new Dictionary<PropertyInfo, Type>();
            DestType = destType;
            RegistrationName = registrationName;
        }
        public List<PropertyInfo> PropertyInjections { get; private set; }
        public Type DestType { get; private set; }
        public string RegistrationName { get; private set; }
        public Dictionary<PropertyInfo,Type> PropertyInjectionResolvers { get; set; }


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
    }
}