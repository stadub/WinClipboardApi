using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.TypeMapping.ValueResolvers;

namespace Utils.TypeMapping.PropertyMappers
{
    public class IgnorePropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, IPropertyMappingInfo propInfo, object sourceValue, IList<Attribute> metadata = null)
        {
            return metadata.Any(attribute => attribute is NonSerializableAttribute);
        }
    }
}
