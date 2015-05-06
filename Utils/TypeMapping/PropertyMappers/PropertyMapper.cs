using System.Reflection;

namespace Utils.TypeMapping.PropertyMappers
{
    public class PropertyMapper : IPropertyMapper
    {
        public bool MapPropery(ITypeMapper mapper, PropertyInfo propInfo, object sourceValue, object instance)
        {

            var mappingResult = mapper.Map(sourceValue, propInfo.PropertyType);
            if (mappingResult.Success)
            {
                propInfo.SetValue(instance, mappingResult.Value);
                return true;
            }
            return false;
        }
    }
}
