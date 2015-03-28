using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ServiceLocatorInfo;

namespace Utils
{
    public interface ITypeMapperRegistry
    {
        IPropertyRegistrationInfo<TDest> Register<TSource, TDest>();
        void Register(ITypeMapper mapper);
        object Resolve(object source, Type destType);
        TDest Resolve<TDest>(object source);
    }

    public class TypeMapperRegistry : ITypeMapperRegistry
    {
        protected Dictionary<KeyValuePair<string, string>, ITypeMapper> mappingDictionary = new Dictionary<KeyValuePair<string, string>, ITypeMapper>();


        protected static KeyValuePair<string, string> GetDictionaryKey(Type sourceType, Type destType)
        {
            return new KeyValuePair<String, String>(sourceType.FullName, destType.FullName);
        }

        public IPropertyRegistrationInfo<TDest> Register<TSource, TDest>() 
        {
            var typeBuilder = new TypeMapper<TSource,TDest>();
            Register(typeBuilder);

            return typeBuilder.MappingInfo;
        }

        public void Register(ITypeMapper mapper)
        {
            var mappingKey = GetDictionaryKey(mapper.SourceType, mapper.DestType);
            if (mappingDictionary.ContainsKey(mappingKey))
                throw new TypeAllreadyRegisteredException(mapper.SourceType);

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
                throw new TypeNotResolvedException(sourceType, "Type mapping doesn't exist in the registry");
            var typeBuilder = mappingDictionary[mappingKey];
            return typeBuilder.Map(source);
        }

        public IEnumerable<TDest> ResolveDestTypeDescendants<TDest>(object source)
        {
            var destType = typeof(TDest);
            return ResolveDestTypeDescendants(source, destType).Cast<TDest>();
        }

        public IEnumerable<object> ResolveDestTypeDescendants(object source, Type destType)
        {
            var sourceType = source.GetType();
            foreach (var typeMapper in mappingDictionary)
            {
                var registeredType = typeMapper.Key;
                var registeredDestType = typeMapper.Value.DestType;

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