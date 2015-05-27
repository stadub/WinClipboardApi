using System;
using System.Collections.Generic;
using System.Reflection;

namespace Utils.TypeMapping.PropertyMappers
{
    public class PropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, PropertyInfo propInfo, object sourceValue, object instance, IList<Attribute> metadata = null)
        {
            IOperationResult mappingResult;
            if (mapper is ITypeInfoMapper)
                mappingResult = ((ITypeInfoMapper)mapper).Map(new SourceInfo(sourceValue) { Attributes = metadata }, propInfo.PropertyType);   
            else
                mappingResult = mapper.Map(sourceValue, propInfo.PropertyType);
            if (mappingResult.Success)
            {
                propInfo.SetValue(instance, mappingResult.Value);
                return true;
            }
            return false;
        }

    }
}
