using System;
using System.Linq.Expressions;

namespace Utils.TypeMapping.MappingInfo
{
    public interface ILocatorResolutionInfo<TClass> : ILocatorRegistrationInfo<TClass>, IPropertyRegistrationInfo<TClass>
    {
    }

    public interface ITypeMappingInfo<TClass> : IPropertyMappingInfo<TClass>, IPropertyRegistrationInfo<TClass>
    {
    }

    public interface ILocatorRegistrationInfo<TClass>
    {
        void InjectProperty<TProp>(Expression<Func<TClass, TProp>> poperty);
        void InjectNamedProperty<TProp>(Expression<Func<TClass, TProp>> poperty, string reristeredName);
    }

    public interface IPropertyRegistrationInfo<TClass>
    {
        void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value);
        void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> poperty);
    }

    public interface IPropertyMappingInfo<TClass>
    {
        void MapProperty<TSourceProp, TProp>(Expression<Func<TClass, TSourceProp, TProp>> poperty, ITypeMapper<TSourceProp, TProp> mapper);
    }
}
