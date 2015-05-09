using System;
using System.Collections.Generic;
using Utils.TypeMapping.TypeMappers;

namespace Utils.TypeMapping
{
    public class DictionaryMapper<TSource, TDest> : TypeMapperBase<IDictionary<string,TSource>, TDest>
    {
        public DictionaryMapper()
        {
            DestType = typeof(TDest);
        }
        public bool LocatorInjectOnlyFlaggedProperties { get;set; }

        public override IOperationResult<TDest> TryMap(IDictionary<string, TSource> source)
        {
            var mapper = CreateTypeBuilder();

            var ctor = mapper.GetConstructor();

            mapper.CreateBuildingContext();
            mapper.InitBuildingContext();
            mapper.Context.Source = source;
            mapper.CreateInstance(ctor, DestType.FullName);
            mapper.CallInitMethods();

            mapper.InjectTypeProperties();

            return OperationResult<TDest>.Successful(mapper.Context.Instance);
        }

        protected virtual DictionaryMappingTypeBuilder<TSource, TDest> CreateTypeBuilder()
        {
            if (DestType.IsGenericType || DestType.IsGenericType)
                throw new TypeNotSupportedException(DestType.FullName, "Generic types are not supported");

            var mapper = new DictionaryMappingTypeBuilder<TSource, TDest>();
            return mapper;
        }

        public Type DestType { get; private set; }
    }


}
