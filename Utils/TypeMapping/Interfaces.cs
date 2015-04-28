using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils.TypeMapping
{
    public interface ITypeMapper
    {
        IOperationResult<object> Map(object source,Type destType);
    }

    public interface ITypeMapper<in TSource, out TDest> : ITypeMapper
    {
        IOperationResult<TDest> TryMap(TSource source);
        TDest Map(TSource source);
    }

    public interface ISourceMappingResolver
    {
        bool IsMemberSuitable(PropertyInfo mappingMember);
        bool IsMemberSuitable(ParameterInfo mappingMember);

        OperationResult<object> ResolveSourceValue(PropertyInfo mappingMember);
        OperationResult<object> ResolveSourceValue(ParameterInfo mappingMember);
        
    }

}
