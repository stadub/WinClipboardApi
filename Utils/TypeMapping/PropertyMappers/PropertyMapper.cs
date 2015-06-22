using System;
using System.Collections.Generic;
using System.Reflection;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.PropertyMappers
{
    public class PropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, IPropertyMappingInfo propInfo, object sourceValue, IList<Attribute> metadata = null)
        {
            if (!mapper.CanMap(sourceValue, propInfo.Type))
                return false;
            IOperationResult mappingResult;
            if (mapper is ITypeInfoMapper)
                mappingResult = ((ITypeInfoMapper)mapper).Map(new SourceInfo(sourceValue) { Attributes = metadata }, propInfo.Type);   
            else
                mappingResult = mapper.Map(sourceValue, propInfo.Type);
            if (mappingResult.Success)
            {
                propInfo.SetValue( mappingResult.Value);
                return true;
            }
            return false;
        }

    }
}
