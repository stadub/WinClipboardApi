using System;

namespace Utils.TypeMapping.TypeMappers
{
    public class MappingFunc<TSource, TDest> : TypeMapperBase<TSource, TDest>
    {
        private readonly Func<TSource,TDest> mapper;

        public MappingFunc(Func<TSource,TDest> mapper)
        {
            this.mapper = mapper;
        }


        public override IOperationResult<TDest> TryMap(TSource source)
        {
            var map=mapper(source);
            return OperationResult<TDest>.Successful(map);
        }
    }
}