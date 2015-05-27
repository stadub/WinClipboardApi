using System;
using System.Collections.Generic;
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

        IOperationResult ITypeMapper.Map(object source, Type destType)
        {
            Debugger.Assert(() => destType.IsAssignableFrom(typeof(TDest)), "Incorrect mapping DestType specified.");
            return (IOperationResult<object>)TryMap((TSource)source);
        }

        public bool CanMap(object source, Type destType)
        {
            if (!destType.IsAssignableFrom(typeof(TDest))) return false;
            if (!(source is TSource)) return false;
            return CanMap((TSource)source);
        }

        public abstract bool CanMap(TSource source);
    }
}