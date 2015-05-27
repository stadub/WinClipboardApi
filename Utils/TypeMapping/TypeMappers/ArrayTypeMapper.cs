using System.Collections.Generic;
using System.Linq;

namespace Utils.TypeMapping.TypeMappers
{
    public class ArrayTypeMapper<TSource, TDest> : TypeMapperBase<IEnumerable<TSource>, IList<TDest>>
    {
        private readonly ITypeMapper<TSource, TDest> elementMapper;

        public ArrayTypeMapper(ITypeMapper<TSource, TDest> elementMapper)
        {
            this.elementMapper = elementMapper;
        }

        public override bool CanMap(IEnumerable<TSource> source)
        {
            return elementMapper.CanMap(source.First());
        }

        public override IOperationResult<IList<TDest>> TryMap(IEnumerable<TSource> source)
        {
            var sourceArray = source.ToArray();
            var destArray = new TDest[sourceArray.Length];
            for (int i = 0; i < destArray.Length; i++)
            {
                var sourceItem = sourceArray[i];
                var mappingValue = elementMapper.TryMap(sourceItem);
                if (!mappingValue.Success)
                    return OperationResult<IList<TDest>>.Failed();
                destArray[i] = mappingValue.Value;
            }
            return OperationResult<IList<TDest>>.Successful(destArray);
        }
    }

    public class ArrayTypeMapper
    {
        public static ArrayTypeMapper<TSource, TDest> Create<TSource, TDest>(ITypeMapper<TSource, TDest> elementMapper)
        {
            return new ArrayTypeMapper<TSource, TDest>(elementMapper);
        }
    }
}
