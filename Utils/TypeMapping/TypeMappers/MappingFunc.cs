using System;
using System.Collections.Generic;

namespace Utils.TypeMapping.TypeMappers
{
    public class MappingFunc<TSource, TDest> : TypeMapperBase<TSource, TDest>
    {
        private readonly Func<TSource,TDest> mapper;
        private readonly Predicate<TSource> canMap;

        public MappingFunc(Func<TSource, TDest> mapper,Predicate<TSource> canMap=null)
        {
            this.mapper = mapper;
            this.canMap = canMap;
        }

        public override bool CanMap(TSource source)
        {
            if (canMap == null) return true;
            return canMap(source);
        }

        public override IOperationResult<TDest> TryMap(TSource source)
        {
            var map=mapper(source);
            return OperationResult<TDest>.Successful(map);
        }
    }
}