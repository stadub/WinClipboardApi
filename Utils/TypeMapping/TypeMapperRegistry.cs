using System;
using System.Collections.Generic;
using System.Linq;
using Utils.TypeMapping.MappingInfo;

namespace Utils.TypeMapping
{

    public class TypeMapperRegistry : ITypeMapperRegistry
    {
        protected Dictionary<KeyValuePair<string, Type>, ITypeMapper> mappingDictionary = new Dictionary<KeyValuePair<string, Type>, ITypeMapper>();


        protected static KeyValuePair<string, Type> GetDictionaryKey(Type sourceType, Type destType)
        {
            return new KeyValuePair<String, Type>(sourceType.FullName, destType);
        }

        public IPropertyRegistrationInfo<TDest> Register<TSource, TDest>() 
        {
            var typeBuilder = new TypeMapper<TSource,TDest>();
            Register<TSource, TDest>(typeBuilder);

            return typeBuilder.RegistrationInfo;
        }

        public void Register<TSource,TDest>(ITypeMapper mapper)
        {
            var sourceType = typeof (TSource);
            var destType = typeof(TDest);

            var mappingKey = GetDictionaryKey(sourceType, destType);
            if (mappingDictionary.ContainsKey(mappingKey))
                throw new TypeAllreadyRegisteredException(sourceType.FullName);

            mappingDictionary.Add(mappingKey, mapper);
        }

        public TDest Resolve<TDest>(object source)
        {
            var destType = typeof(TDest);
            return (TDest) Resolve(source, destType);
        }


        public object Resolve(object source,Type destType)
        {
            var sourceType = source.GetType();
            var mappingKey = GetDictionaryKey(sourceType, destType);
            if (!mappingDictionary.ContainsKey(mappingKey))
                throw new TypeNotResolvedException(sourceType.FullName, "Type mapping doesn't exist in the registry");
            var typeBuilder = mappingDictionary[mappingKey];
            var result= typeBuilder.Map(source, destType);
            return result.Value;
        }

        public IEnumerable<TDest> ResolveDescendants<TDest>(object source)
        {
            var destType = typeof(TDest);
            return ResolveDescendants(source, destType).Cast<TDest>();
        }

        public IEnumerable<object> ResolveDescendants(object source, Type destType)
        {
            var sourceType = source.GetType();
            foreach (var typeMapper in mappingDictionary)
            {
                var registeredType = typeMapper.Key;
                var registeredDestType = typeMapper.Key.Value;

                if(sourceType.FullName!=registeredType.Key)
                    continue;
                if(!destType.IsAssignableFrom(registeredDestType))
                    continue;
                var result=Resolve(source, registeredDestType);
                yield return result;
            }
        }
    }
}