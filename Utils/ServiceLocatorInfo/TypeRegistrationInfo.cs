using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.ServiceLocatorInfo
{
    internal class TypeRegistrationInfo
    {
        public TypeRegistrationInfo(Type destType, string registrationName)
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
    }
}