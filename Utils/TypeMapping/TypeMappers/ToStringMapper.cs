using System;

namespace Utils.TypeMapping.TypeMappers
{
    public class ToStringMapper<TSource> : TypeMapperBase<TSource, string>, ITypeInfoMapper
    {

        public IOperationResult Map(ISourceInfo sourceInfo, Type destType)
        {
            if (!destType.IsAssignableFrom(typeof(string))) return OperationResult.Failed();

            return TryMap(sourceInfo) as IOperationResult<string>;
        }

        public IOperationResult TryMap(ISourceInfo sourceInfo)
        {
            return sourceInfo.Value == null ? OperationResult<string>.Failed() : OperationResult<string>.Successful(sourceInfo.Value.ToString());
        }

        public override bool CanMap(TSource source)
        {
            return true;
        }

        public override IOperationResult<string> TryMap(TSource source)
        {
            return (IOperationResult<string>) TryMap(new SourceInfo(source));
        }
    }
}
