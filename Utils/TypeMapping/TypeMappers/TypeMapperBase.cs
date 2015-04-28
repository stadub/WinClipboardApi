using System;
using Utils.Helpers;

namespace Utils.TypeMapping.TypeMappers
{
    public abstract class TypeMapperBase<TSource, TDest> : ITypeMapper<TSource, TDest>
    {
        public abstract IOperationResult<TDest> TryMap(TSource source);

        public TDest Map(TSource source)
        {
            var mapResult = TryMap(source);
            return mapResult.Success ? mapResult.Value : default(TDest);
        }

        IOperationResult<object> ITypeMapper.Map(object source, Type destType)
        {
            Debugger.Assert(() => destType.IsAssignableFrom(typeof(TDest)), "Incorrect mapping DestType specified.");
            return (IOperationResult<object>) TryMap((TSource) source);
        }
    }
}