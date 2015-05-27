using System;
using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping
{
    public interface ITypeMapper
    {
        IOperationResult Map(object source,Type destType);
        bool CanMap(object source,Type destType);
    }
    
    public interface ITypeInfoMapper
    {
        IOperationResult Map(ISourceInfo sourceInfo,Type destType);
        IOperationResult TryMap(ISourceInfo sourceInfo);
    }

    public interface ITypeMapper<in TSource, out TDest> : ITypeMapper
    {
        bool CanMap(TSource source);
        IOperationResult<TDest> TryMap(TSource source);
        TDest Map(TSource source);
    }

    public interface ISourceMappingResolver
    {
        bool IsMemberSuitable(PropertyInfo propertyInfo);
        bool IsMemberSuitable(ParameterInfo mappingMember);

        ISourceInfo ResolveSourceValue(PropertyInfo propertyInfo, object source);
        ISourceInfo ResolveSourceValue(ParameterInfo mappingMember, object source);
    }

    public interface ISourceInfo
    {
        Object Value { get; }
        IList<Attribute> Attributes { get; }
    }

    public interface IPropertyMapper
    {
        bool MapPropery(ITypeMapper mapper, PropertyInfo property, object value, object instance, IList<Attribute> metadata = null);
    }

    public interface ITypeMapperRegistry
    {
        ITypeMappingInfo<TDest> Register<TSource, TDest>();
        void Register<TSource, TDest>(ITypeMapper mapper);

        object Resolve(object source, Type destType);
        TDest Resolve<TDest>(object source);

        IEnumerable<TDest> ResolveDescendants<TDest>(object source);
        IEnumerable<object> ResolveDescendants(object source, Type destType);
    }
}
