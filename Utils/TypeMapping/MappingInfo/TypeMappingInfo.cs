using System;
using System.Linq.Expressions;

namespace Utils.TypeMapping.MappingInfo
{
    public class TypeMappingInfo<TClass> : ITypeMappingInfo<TClass>
    {
        private readonly IPropertyMappingInfo<TClass> mappingInfo;
        private readonly IPropertyRegistrationInfo<TClass> registrationInfo;

        public TypeMappingInfo(IPropertyMappingInfo<TClass> mappingInfo, IPropertyRegistrationInfo<TClass> registrationInfo)
        {
            this.mappingInfo = mappingInfo;
            this.registrationInfo = registrationInfo;
        }

        public void MapProperty<TSourceProp, TProp>(Expression<Func<TClass, TSourceProp, TProp>> poperty, ITypeMapper<TSourceProp, TProp> mapper)
        {
            mappingInfo.MapProperty(poperty,mapper);
        }

        public void InjectPropertyValue<TProp>(Expression<Func<TClass, TProp>> poperty, TProp value)
        {
            registrationInfo.InjectPropertyValue(poperty,value);
        }

        public void IgnoreProperty<TProp>(Expression<Func<TClass, TProp>> poperty)
        {
            registrationInfo.IgnoreProperty(poperty);
        }
    }
}