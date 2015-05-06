using System;
using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping.MappingInfo;

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
        bool IsMemberSuitable(PropertyInfo propertyInfo);
        bool IsMemberSuitable(ParameterInfo mappingMember);

        OperationResult ResolveSourceValue(PropertyInfo propertyInfo, object source);
        OperationResult ResolveSourceValue(ParameterInfo mappingMember, object source);
    }

    public interface IPropertyMapper
    {
        bool MapPropery(ITypeMapper mapper, PropertyInfo property, object value, object instance);
    }

    public interface ITypeMapperRegistry
    {
        IPropertyRegistrationInfo<TDest> Register<TSource, TDest>();
        void Register<TSource, TDest>(ITypeMapper mapper);

        object Resolve(object source, Type destType);
        TDest Resolve<TDest>(object source);

        IEnumerable<TDest> ResolveDescendants<TDest>(object source);
        IEnumerable<object> ResolveDescendants(object source, Type destType);
    }


}
